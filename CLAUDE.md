# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Baby Tracker is a comprehensive application for parents to monitor and log their baby's daily activities and milestones. The app tracks:
- **Sleep**: bedtime, wake time, duration, and quality notes
- **Growth**: weight, length, head circumference measurements
- **Nappies**: diaper changes with type (wet/dirty) and notes
- **Milk/Feeding**: bottle feeding, breastfeeding, and introduction to solids

## Architecture

### Frontend
- **Framework**: React or Next.js
- **State Management**: Context API or Redux
- **Styling**: Tailwind CSS or styled-components
- **Key Pages**:
  - Dashboard: overview of recent activities
  - Add Activity: forms for logging sleep, growth, nappies, feeding
  - History: timeline view of all activities
  - Baby Profile: baby information and growth charts
  - Settings: app preferences and data export

### Backend
- **Runtime**: .NET (ASP.NET Core)
- **Language**: C#
- **Data Storage**: Local JSON file (`data.json`)
- **Core Data Structure**:
  ```json
  {
    "babies": [],
    "activities": [],
    "users": []
  }
  ```

### Data Model
- All activities (sleep, growth, nappies, feeding) stored with type discriminator
- Each activity tied to a specific baby
- Timestamps always in UTC for consistency
- Support multiple caregivers per baby

## Development Commands

### Project Setup
```bash
# Create a new ASP.NET Core Web API project (if not already created)
dotnet new webapi -n BabyTrackerApi

# Restore dependencies
dotnet restore

# Build the solution
dotnet build
```

### Development
```bash
# Run the application in development mode
dotnet run

# Run with watch mode (auto-restart on file changes)
dotnet watch run
```

### Testing
```bash
# Run all tests
dotnet test

# Run tests with verbose output
dotnet test --verbosity normal

# Run specific test project
dotnet test path/to/TestProject.csproj
```

### Building
```bash
# Build for production
dotnet build --configuration Release

# Publish application
dotnet publish --configuration Release --output ./publish
```

## Project Structure

```
baby-tracker/
├── BabyTrackerApi/              # Main API project
│  ├── Controllers/              # API endpoints
│  │  ├── BabiesController.cs
│  │  ├── ActivitiesController.cs
│  │  └── ...
│  ├── Models/                   # Data models
│  │  ├── Baby.cs
│  │  ├── Activity.cs
│  │  ├── Sleep.cs
│  │  ├── Growth.cs
│  │  ├── Nappy.cs
│  │  └── Feeding.cs
│  ├── Services/                 # Business logic
│  │  ├── DataStore.cs          # Handles JSON file I/O
│  │  ├── BabyService.cs
│  │  ├── ActivityService.cs
│  │  └── ...
│  ├── Program.cs                # Startup configuration
│  ├── appsettings.json
│  └── BabyTrackerApi.csproj
├── BabyTrackerApi.Tests/        # Unit tests
│  ├── Services/
│  ├── Controllers/
│  └── BabyTrackerApi.Tests.csproj
├── data.json                    # Local data file (auto-created)
├── .gitignore
└── CLAUDE.md

```

## Key Implementation Details

### Data Storage (JSON File)
- **DataStore.cs**: Core service managing all JSON file operations
  - Read/Write operations with file locking to prevent concurrent access issues
  - Serialize/deserialize data models
  - Methods: `ReadData()`, `WriteData(data)`, `GetBabies()`, `AddActivity()`, etc.
- Single source of truth: `data.json` in project root
- Initialize with empty structure if file doesn't exist
- Consider backup strategy for safety

### Activity Types
- Base `Activity` class with common fields: `Id`, `BabyId`, `Timestamp`, `Type`
- Derived classes for each activity type: `Sleep`, `Growth`, `Nappy`, `Feeding`
- Each activity stores type-specific data (e.g., `Sleep` has `BedTime`, `WakeTime`, `Quality`)

### API Endpoints (ASP.NET Core)
Structure:
```
GET    /api/babies              - List all babies
POST   /api/babies              - Create new baby
GET    /api/babies/{id}         - Get baby details

GET    /api/activities?babyId=  - List activities for a baby
POST   /api/activities          - Log new activity
PUT    /api/activities/{id}     - Update activity
DELETE /api/activities/{id}     - Delete activity

GET    /api/growth/{id}         - Get growth chart data for baby
```

### Timestamp Handling
- Store all timestamps in UTC with ISO 8601 format
- Parse incoming timestamps carefully (may be in different timezones)
- Display logic handles timezone conversion on frontend

### Common Operations

#### Adding a New Activity
1. Create model class in `Models/` (e.g., `Feeding.cs` extending `Activity`)
2. Add endpoint in `ActivitiesController.cs`
3. Add service method in `ActivityService.cs` to append to `data.json`
4. Validate required fields before saving

#### Modifying Data Structure
- Update model classes
- No migrations needed (JSON is flexible)
- Ensure backward compatibility: old records should still deserialize
- Add default values for new fields in models

### Data Validation
- Validate timestamps are not in the future
- Required fields: `BabyId`, `Timestamp`, `Type`
- Activity-specific validation (e.g., `Growth`: weight/length > 0)
- Handle missing `data.json` gracefully (initialize on startup)

### Concurrency Handling
- Add file locking in `DataStore.cs` to prevent corrupted writes
- Use `Mutex` or `ReaderWriterLockSlim` for thread-safe access
- Brief locks—read data, process in memory, write back atomically

## Environment Configuration

Default locations:
- **Data file**: `./data.json` (project root)
- **Log file** (optional): `./logs/app.log`

Configurable via `appsettings.json`:
```json
{
  "DataStore": {
    "FilePath": "./data.json"
  }
}
```

## Backup & Safety

- Manually backup `data.json` periodically
- Consider adding API endpoint for data export
- On startup, create timestamped backup: `data.backup.{timestamp}.json`

## Common Development Tasks

### Running the App Locally
```bash
# Terminal 1: Run .NET backend
dotnet watch run

# Terminal 2: Run React frontend
npm run dev
```

### Testing Activity Logging
- Use Postman or curl to test endpoints
- Verify activities appear in `data.json`
- Test with multiple babies and activity types
- Verify timestamps are stored in UTC

### Checking Data File
```bash
# View contents of data.json (pretty-printed)
cat data.json | jq
```

---

**Note**: Keep this file updated as the project evolves, especially when adding new activity types or changing the data structure.
