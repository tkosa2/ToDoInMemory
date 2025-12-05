# ToDo In Memory - Blazor .NET Application

A beautiful, modern ToDo application built with Blazor Server and C# using in-memory storage.

## Features

- ✅ Add, edit, and delete tasks
- ✅ Mark tasks as complete/incomplete
- ✅ Filter tasks (All, Pending, Completed)
- ✅ Real-time task counter statistics
- ✅ Modern dark theme UI
- ✅ Responsive design
- ✅ Browser localStorage persistence

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later

## Getting Started

1. Navigate to the project directory:
   ```bash
   cd ToDoInMemory
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. Open your browser and navigate to:
   - https://localhost:7001 (HTTPS)
   - http://localhost:5001 (HTTP)

## Project Structure

```
ToDoInMemory/
├── Components/
│   ├── Layout/
│   │   └── MainLayout.razor
│   ├── Pages/
│   │   └── Home.razor
│   ├── App.razor
│   ├── Routes.razor
│   └── _Imports.razor
├── Models/
│   └── TodoItem.cs
├── Services/
│   └── TodoService.cs
├── Properties/
│   └── launchSettings.json
├── wwwroot/
│   └── css/
│       └── app.css
├── appsettings.json
├── Program.cs
└── ToDoInMemory.csproj
```

## Usage

- **Add a task**: Type in the input field and click "Add Task" or press Enter
- **Complete a task**: Click the circular checkbox on the left
- **Edit a task**: Double-click on the task text or click the edit (✏️) button
- **Delete a task**: Click the delete (🗑️) button
- **Filter tasks**: Use the filter tabs to show All, Pending, or Completed tasks

## Technology Stack

- Blazor Server (.NET 8)
- C# 12
- CSS3 with custom properties
- Google Fonts (Outfit)

## Note

This application uses browser localStorage for data persistence, which means:
- Data persists across browser sessions and page refreshes
- Data is stored locally in the user's browser
- Each browser session maintains its own data
- Clearing browser data will remove all tasks

For production use with multiple users and server-side data management, consider implementing a database backend (Entity Framework Core with SQL Server, SQLite, or PostgreSQL).

