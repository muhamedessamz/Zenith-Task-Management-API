# 🤝 Contributing to Zenith Task Management System

First off, thank you for considering contributing to Zenith Task Management System! It's people like you that make this project better for everyone.

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How Can I Contribute?](#how-can-i-contribute)
- [Development Setup](#development-setup)
- [Coding Standards](#coding-standards)
- [Commit Guidelines](#commit-guidelines)
- [Pull Request Process](#pull-request-process)
- [Reporting Bugs](#reporting-bugs)
- [Suggesting Features](#suggesting-features)

---

## 📜 Code of Conduct

This project and everyone participating in it is governed by our Code of Conduct. By participating, you are expected to uphold this code. Please report unacceptable behavior to the project maintainers.

### Our Standards

- ✅ Be respectful and inclusive
- ✅ Welcome newcomers and help them learn
- ✅ Focus on what is best for the community
- ✅ Show empathy towards other community members
- ❌ No harassment, trolling, or insulting comments
- ❌ No political or off-topic discussions

---

## 🚀 How Can I Contribute?

### 1. Reporting Bugs 🐛

Before creating bug reports, please check existing issues to avoid duplicates.

**When submitting a bug report, include:**
- Clear and descriptive title
- Steps to reproduce the issue
- Expected behavior vs actual behavior
- Screenshots (if applicable)
- Environment details (OS, .NET version, database version)
- Error messages and stack traces

**Template:**
```markdown
**Describe the bug**
A clear description of what the bug is.

**To Reproduce**
Steps to reproduce the behavior:
1. Go to '...'
2. Click on '....'
3. See error

**Expected behavior**
What you expected to happen.

**Screenshots**
If applicable, add screenshots.

**Environment:**
 - OS: [e.g. Windows 11]
 - .NET Version: [e.g. 8.0]
 - Database: [e.g. SQL Server 2022]
```

### 2. Suggesting Features 💡

We love feature suggestions! Before creating a feature request:
- Check if the feature already exists
- Check if it's already been suggested
- Consider if it fits the project's scope

**Template:**
```markdown
**Is your feature request related to a problem?**
A clear description of the problem.

**Describe the solution you'd like**
A clear description of what you want to happen.

**Describe alternatives you've considered**
Any alternative solutions or features you've considered.

**Additional context**
Add any other context or screenshots about the feature request.
```

### 3. Code Contributions 💻

We welcome code contributions! Here's how:

1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/AmazingFeature`)
3. **Make your changes**
4. **Write/update tests**
5. **Commit your changes** (see [Commit Guidelines](#commit-guidelines))
6. **Push to your fork** (`git push origin feature/AmazingFeature`)
7. **Open a Pull Request**

---

## 🛠️ Development Setup

### Prerequisites

- .NET 8.0 SDK
- SQL Server 2019+
- Visual Studio 2022 or VS Code
- Git

### Setup Steps

1. **Clone your fork**
   ```bash
   git clone https://github.com/YOUR_USERNAME/Zenith-Task-Management-API.git
   cd Zenith-Task-Management-API
   ```

2. **Add upstream remote**
   ```bash
   git remote add upstream https://github.com/muhamedessamz/Zenith-Task-Management-API.git
   ```

3. **Configure secrets** (see [DEPLOYMENT.md](DEPLOYMENT.md))
   ```bash
   cd TaskManagement.Api
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_CONNECTION_STRING"
   dotnet user-secrets set "JwtSettings:SecretKey" "YourSecretKey32CharsMin!"
   ```

4. **Restore packages**
   ```bash
   dotnet restore
   ```

5. **Apply migrations**
   ```bash
   dotnet ef database update --project TaskManagement.Infrastructure
   ```

6. **Run the application**
   ```bash
   cd TaskManagement.Api
   dotnet run
   ```

7. **Access Swagger**
   ```
   https://localhost:7287/swagger
   ```

---

## 📝 Coding Standards

### C# Style Guide

We follow the [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).

#### Key Points:

1. **Naming Conventions**
   ```csharp
   // PascalCase for classes, methods, properties
   public class TaskService { }
   public void GetTaskById() { }
   public string TaskTitle { get; set; }
   
   // camelCase for local variables and parameters
   var taskItem = new TaskItem();
   public void CreateTask(string taskTitle) { }
   
   // _camelCase for private fields
   private readonly ITaskRepository _taskRepository;
   ```

2. **Async/Await**
   ```csharp
   // Always use async/await for I/O operations
   public async Task<TaskItem> GetTaskByIdAsync(int id)
   {
       return await _repository.GetByIdAsync(id);
   }
   
   // Suffix async methods with "Async"
   ```

3. **Dependency Injection**
   ```csharp
   // Inject dependencies via constructor
   public class TaskService : ITaskService
   {
       private readonly ITaskRepository _repository;
       
       public TaskService(ITaskRepository repository)
       {
           _repository = repository;
       }
   }
   ```

4. **Error Handling**
   ```csharp
   // Use specific exceptions
   if (task == null)
       throw new NotFoundException($"Task with ID {id} not found");
   
   // Don't swallow exceptions
   try
   {
       // code
   }
   catch (Exception ex)
   {
       _logger.LogError(ex, "Error message");
       throw; // Re-throw or handle appropriately
   }
   ```

5. **Comments**
   ```csharp
   // Use XML documentation for public APIs
   /// <summary>
   /// Gets a task by its unique identifier.
   /// </summary>
   /// <param name="id">The task ID</param>
   /// <returns>The task if found, null otherwise</returns>
   public async Task<TaskItem?> GetTaskByIdAsync(int id)
   ```

### Project Structure

```
TaskManagement.Core/          # Domain layer (entities, interfaces)
TaskManagement.Infrastructure/ # Data access layer (repositories, DbContext)
TaskManagement.Services/       # Business logic layer
TaskManagement.Api/            # Presentation layer (controllers, DTOs)
```

### Testing

- Write unit tests for business logic
- Write integration tests for API endpoints
- Aim for >80% code coverage
- Use meaningful test names: `MethodName_Scenario_ExpectedResult`

```csharp
[Fact]
public async Task GetTaskById_WithValidId_ReturnsTask()
{
    // Arrange
    var taskId = 1;
    
    // Act
    var result = await _service.GetTaskByIdAsync(taskId);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(taskId, result.Id);
}
```

---

## 📝 Commit Guidelines

We follow [Conventional Commits](https://www.conventionalcommits.org/).

### Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, no logic change)
- `refactor`: Code refactoring
- `perf`: Performance improvements
- `test`: Adding or updating tests
- `chore`: Maintenance tasks
- `ci`: CI/CD changes

### Examples

```bash
# Feature
git commit -m "feat(tasks): add task filtering by priority"

# Bug fix
git commit -m "fix(auth): resolve JWT token expiration issue"

# Documentation
git commit -m "docs(readme): update installation instructions"

# Refactoring
git commit -m "refactor(services): extract email service interface"

# With body
git commit -m "feat(notifications): add real-time notifications

Implemented SignalR hub for real-time task updates.
Users now receive instant notifications when tasks are assigned.

Closes #123"
```

---

## 🔄 Pull Request Process

### Before Submitting

1. ✅ Ensure your code follows the coding standards
2. ✅ Write/update tests for your changes
3. ✅ Update documentation if needed
4. ✅ Run all tests and ensure they pass
5. ✅ Rebase on latest `main` branch
6. ✅ Ensure no merge conflicts

### PR Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## How Has This Been Tested?
Describe the tests you ran

## Checklist
- [ ] My code follows the project's coding standards
- [ ] I have performed a self-review of my code
- [ ] I have commented my code where necessary
- [ ] I have updated the documentation
- [ ] My changes generate no new warnings
- [ ] I have added tests that prove my fix/feature works
- [ ] New and existing tests pass locally

## Screenshots (if applicable)
Add screenshots to help explain your changes

## Related Issues
Closes #(issue number)
```

### Review Process

1. At least one maintainer must approve the PR
2. All CI checks must pass
3. No merge conflicts
4. Code review feedback must be addressed

---

## 🏷️ Branching Strategy

- `main` - Production-ready code
- `develop` - Integration branch for features
- `feature/*` - New features
- `fix/*` - Bug fixes
- `hotfix/*` - Urgent production fixes
- `docs/*` - Documentation updates

### Example Workflow

```bash
# Create feature branch from main
git checkout main
git pull upstream main
git checkout -b feature/add-task-tags

# Make changes and commit
git add .
git commit -m "feat(tasks): add tag support for tasks"

# Push to your fork
git push origin feature/add-task-tags

# Create PR on GitHub
```

---

## 🐛 Debugging Tips

### Enable Detailed Logging

In `appsettings.Development.json`:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  }
}
```

### Common Issues

1. **Database Connection Errors**
   - Verify connection string
   - Ensure SQL Server is running
   - Check firewall rules

2. **JWT Token Issues**
   - Verify SecretKey is at least 16 characters
   - Check token expiration time
   - Ensure Issuer and Audience match

3. **Migration Errors**
   - Delete database and recreate
   - Check for pending migrations
   - Verify EF Core tools are installed

---

## 📚 Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [REST API Best Practices](https://restfulapi.net/)

---

## 🎉 Recognition

Contributors will be recognized in:
- README.md Contributors section
- Release notes
- Project documentation

---

## 📞 Questions? 

- Open a [Discussion](https://github.com/muhamedessamz/Zenith-Task-Management-API/discussions)
- Join our community chat
- LinkedIn: [Mohamed Essam](https://www.linkedin.com/in/mohamedessamz/)
- Email: muhamedessamz@gmail.com

---

<div align="center">

**Thank you for contributing! 🙏**

Every contribution, no matter how small, makes a difference.

</div>
