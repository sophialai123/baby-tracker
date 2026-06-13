using System.Text.Json;
using BabyTrackerApi.Models;

namespace BabyTrackerApi.Services;

public class DataStore
{
    private readonly string _filePath;
    private readonly ReaderWriterLockSlim _lock = new();
    private const int LockTimeoutMs = 5000;

    public DataStore(IConfiguration configuration)
    {
        var dataStoreConfig = configuration.GetSection("DataStore");
        _filePath = dataStoreConfig["FilePath"] ?? "./data.json";
    }

    private class DataContainer
    {
        public List<Baby> Babies { get; set; } = new();
        public List<Activity> Activities { get; set; } = new();
    }

    public async Task<List<Baby>> GetBabiesAsync()
    {
        return await Task.Run(() =>
        {
            if (!_lock.TryEnterReadLock(LockTimeoutMs))
                throw new TimeoutException("Failed to acquire read lock");

            try
            {
                var data = ReadData();
                return data.Babies;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        });
    }

    public async Task<Baby?> GetBabyAsync(string id)
    {
        return await Task.Run(() =>
        {
            if (!_lock.TryEnterReadLock(LockTimeoutMs))
                throw new TimeoutException("Failed to acquire read lock");

            try
            {
                var data = ReadData();
                return data.Babies.FirstOrDefault(b => b.Id == id);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        });
    }

    public async Task<Baby> AddBabyAsync(Baby baby)
    {
        return await Task.Run(() =>
        {
            if (!_lock.TryEnterWriteLock(LockTimeoutMs))
                throw new TimeoutException("Failed to acquire write lock");

            try
            {
                var data = ReadData();
                data.Babies.Add(baby);
                WriteData(data);
                return baby;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        });
    }

    public async Task<bool> UpdateBabyAsync(string id, Baby updatedBaby)
    {
        return await Task.Run(() =>
        {
            if (!_lock.TryEnterWriteLock(LockTimeoutMs))
                throw new TimeoutException("Failed to acquire write lock");

            try
            {
                var data = ReadData();
                var baby = data.Babies.FirstOrDefault(b => b.Id == id);
                if (baby == null) return false;

                baby.Name = updatedBaby.Name;
                baby.DateOfBirth = updatedBaby.DateOfBirth;
                baby.Gender = updatedBaby.Gender;
                baby.BirthWeight = updatedBaby.BirthWeight;
                baby.BirthLength = updatedBaby.BirthLength;

                WriteData(data);
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        });
    }

    public async Task<bool> DeleteBabyAsync(string id)
    {
        return await Task.Run(() =>
        {
            if (!_lock.TryEnterWriteLock(LockTimeoutMs))
                throw new TimeoutException("Failed to acquire write lock");

            try
            {
                var data = ReadData();
                var baby = data.Babies.FirstOrDefault(b => b.Id == id);
                if (baby == null) return false;

                data.Babies.Remove(baby);
                data.Activities.RemoveAll(a => a.BabyId == id);
                WriteData(data);
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        });
    }

    public async Task<List<Activity>> GetActivitiesAsync(string babyId)
    {
        return await Task.Run(() =>
        {
            if (!_lock.TryEnterReadLock(LockTimeoutMs))
                throw new TimeoutException("Failed to acquire read lock");

            try
            {
                var data = ReadData();
                return data.Activities
                    .Where(a => a.BabyId == babyId)
                    .OrderByDescending(a => a.Timestamp)
                    .ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        });
    }

    public async Task<Activity?> GetActivityAsync(string id)
    {
        return await Task.Run(() =>
        {
            if (!_lock.TryEnterReadLock(LockTimeoutMs))
                throw new TimeoutException("Failed to acquire read lock");

            try
            {
                var data = ReadData();
                return data.Activities.FirstOrDefault(a => a.Id == id);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        });
    }

    public async Task<Activity> AddActivityAsync(Activity activity)
    {
        return await Task.Run(() =>
        {
            if (!_lock.TryEnterWriteLock(LockTimeoutMs))
                throw new TimeoutException("Failed to acquire write lock");

            try
            {
                var data = ReadData();
                data.Activities.Add(activity);
                WriteData(data);
                return activity;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        });
    }

    public async Task<bool> UpdateActivityAsync(string id, Activity updatedActivity)
    {
        return await Task.Run(() =>
        {
            if (!_lock.TryEnterWriteLock(LockTimeoutMs))
                throw new TimeoutException("Failed to acquire write lock");

            try
            {
                var data = ReadData();
                var index = data.Activities.FindIndex(a => a.Id == id);
                if (index == -1) return false;

                data.Activities[index] = updatedActivity;
                WriteData(data);
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        });
    }

    public async Task<bool> DeleteActivityAsync(string id)
    {
        return await Task.Run(() =>
        {
            if (!_lock.TryEnterWriteLock(LockTimeoutMs))
                throw new TimeoutException("Failed to acquire write lock");

            try
            {
                var data = ReadData();
                var activity = data.Activities.FirstOrDefault(a => a.Id == id);
                if (activity == null) return false;

                data.Activities.Remove(activity);
                WriteData(data);
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        });
    }

    private DataContainer ReadData()
    {
        if (!File.Exists(_filePath))
            return new DataContainer();

        try
        {
            var json = File.ReadAllText(_filePath);
            var options = CreateJsonOptions();
            return JsonSerializer.Deserialize<DataContainer>(json, options) ?? new DataContainer();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error reading data: {ex.Message}");
            return new DataContainer();
        }
    }

    private void WriteData(DataContainer data)
    {
        var options = CreateJsonOptions();
        var json = JsonSerializer.Serialize(data, options);
        File.WriteAllText(_filePath, json);
    }

    private JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };
        options.Converters.Add(new ActivityConverter());
        return options;
    }
}
