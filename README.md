# Baby Tracker App

A comprehensive application for parents to track their baby's daily activities including sleep, growth, nappies, and feeding.

## Tech Stack

- **Frontend**: React / Next.js
- **Backend**: ASP.NET Core (.NET)
- **Data Storage**: Local JSON file (`data.json`)

## Quick Start

### Backend Setup
```bash
# Create new ASP.NET Core Web API project
dotnet new webapi -n BabyTrackerApi

# Run the API
dotnet watch run
```

The API will be available at `https://localhost:5001` (or `http://localhost:5000`)

### Frontend Setup
```bash
# Create React/Next.js project
npx create-react-app baby-tracker-frontend
# or
npx create-next-app baby-tracker-frontend

# Install dependencies and run
cd baby-tracker-frontend
npm run dev
```

## Project Structure

See [CLAUDE.md](./CLAUDE.md) for detailed architecture, development commands, and implementation guidelines.

## Features

- 👶 Create and manage baby profiles
- 😴 Log sleep schedules
- 📊 Track growth measurements
- 🩷 Record diaper changes
- 🍼 Log feeding activities
- 📈 View activity history and trends

## Next Steps

1. Initialize the .NET backend project
2. Set up React/Next.js frontend
3. Create the `DataStore` service for JSON file management
4. Build API endpoints for each activity type
5. Connect frontend to backend API

---

For detailed development guidance, see [CLAUDE.md](./CLAUDE.md).
