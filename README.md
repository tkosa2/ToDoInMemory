# ToDo - Blazor .NET Application

A beautiful, modern ToDo application built with Blazor Server and C# using browser localStorage for data persistence.

> **Note**: This project originally started with in-memory storage and was later converted to use browser localStorage with the assistance of GitHub Copilot, demonstrating the evolution of the application and the practical use of AI-assisted development.

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

## Learning Experiences

During the development of this project, two key workflows were implemented and learned:

### 1. Working with GitHub Copilot

This project demonstrates a workflow for collaborating with AI coding assistants:

1. **Create an Issue**: Start by creating a GitHub issue describing the feature, bug fix, or enhancement
2. **Assign to Copilot**: Assign the issue to GitHub Copilot or your AI coding assistant
3. **Implement Changes**: Work on the changes in a feature branch
4. **Code Review**: Review all changes carefully before merging to ensure code quality
5. **Merge to Main**: Only merge after thorough code review

**Real-world example**: This project started with in-memory storage and was successfully converted to use browser localStorage by creating an issue, assigning it to Copilot, and reviewing the changes before merging. This workflow helps maintain code quality while leveraging AI assistance effectively.

### 2. Testing and GitHub Actions Workflow

This project includes comprehensive testing infrastructure and automated CI/CD:

#### Testing Strategy

**Unit Tests**
- Located in `ToDoInMemory.Tests/`
- Test individual components and services in isolation
- Use mocking frameworks (Moq) to isolate dependencies
- Fast execution, no external dependencies

**Integration Tests**
- Located in `ToDoInMemory.Tests/IntegrationTests.cs`
- Test the application end-to-end using **Kestrel web server**
- Use `WebApplicationFactory<Program>` to spin up an in-memory test server
- Make actual HTTP requests to verify application behavior
- Test the full request/response cycle

#### Running Tests Locally

Run all tests:
```bash
dotnet test
```

Run only unit tests:
```bash
dotnet test --filter "FullyQualifiedName!~IntegrationTests"
```

Run only integration tests:
```bash
dotnet test --filter "FullyQualifiedName~IntegrationTests"
```

#### GitHub Actions Workflow

A GitHub Actions workflow has been configured to automatically run tests:

- **Location**: `.github/workflows/test.yml`
- **Triggers**: 
  - When a new branch is pushed to GitHub
  - When a pull request is created or updated
- **Actions**: 
  - Runs both unit tests and integration tests
  - Builds the solution
  - Uploads test results as artifacts

**All tests must pass before creating a pull request** to ensure code quality and prevent errors during code review. The workflow file in the `.github` folder is automatically picked up by GitHub to execute the actions.

## Note

This application uses browser localStorage for data persistence, which means:
- Data persists across browser sessions and page refreshes
- Data is stored locally in the user's browser
- Each browser session maintains its own data
- Clearing browser data will remove all tasks

For production use with multiple users and server-side data management, consider implementing a database backend (Entity Framework Core with SQL Server, SQLite, or PostgreSQL).

