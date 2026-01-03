# 🚀 Zenith Task Management System

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge&logo=c-sharp)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoft-sql-server)
![JWT](https://img.shields.io/badge/JWT-000000?style=for-the-badge&logo=json-web-tokens)
![SignalR](https://img.shields.io/badge/SignalR-512BD4?style=for-the-badge&logo=dotnet)

**A comprehensive, enterprise-grade Task Management System built with ASP.NET Core 8.0**

[Features](#-features) • [Getting Started](#-getting-started) • [API Documentation](#-api-documentation) • [Architecture](#-architecture) • [Contributing](#-contributing)

</div>

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Technology Stack](#-technology-stack)
- [Architecture](#-architecture)
- [Getting Started](#-getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [Configuration](#configuration)
  - [Database Setup](#database-setup)
- [API Documentation](#-api-documentation)
- [Project Structure](#-project-structure)
- [Security](#-security)
- [Performance Optimizations](#-performance-optimizations)
- [Testing](#-testing)
- [Deployment](#-deployment)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🎯 Overview

**Zenith Task Management System** is a robust, scalable, and feature-rich RESTful Web API designed to streamline task management for individuals, teams, and organizations. Built with modern software engineering principles, it provides a comprehensive backend solution for creating, organizing, tracking, and collaborating on tasks.

### 🎪 Problem It Solves

- **Task Overload**: Centralized system for managing multiple tasks across different projects
- **Missed Deadlines**: Advanced tracking, reminders, and notification mechanisms
- **Poor Organization**: Intelligent categorization, tagging, and priority management
- **Team Collaboration**: Real-time updates, task assignments, and project management
- **Accountability**: Complete audit trail with task history and activity logs
  

### 👥 Target Users

- **Individual Users**: Professionals, students, and freelancers managing personal tasks
- **Teams**: Development teams, project managers, and remote teams requiring collaboration
- **Organizations**: Departments needing task delegation, tracking, and analytics

---

## ✨ Features

### 🔐 Authentication & Authorization
- **JWT-based Authentication** with secure token management
- **Role-based Access Control** (Admin, User, Manager)
- **Email Verification** with OTP (One-Time Password)
- **Password Security** with BCrypt hashing
- **Refresh Tokens** for seamless session management
- **Account Management** (activation, deactivation, password reset)

### 📝 Task Management
- **CRUD Operations** with comprehensive validation
- **Task Priorities** (Low, Medium, High, Critical)
- **Task Status Tracking** (Pending, In Progress, Completed)
- **Due Dates & Reminders** with automated notifications
- **Recurring Tasks** (Daily, Weekly, Monthly)
- **Task Dependencies** for complex workflows
- **Checklist Items** for subtask management
- **Task Attachments** with file upload support
- **Task Comments** for collaboration
- **Task History** with complete audit trail
- **Advanced Search & Filtering** with pagination
- **Soft Delete & Restore** functionality

### 🏷️ Organization Features
- **Categories** with color coding
- **Tags** for flexible classification
- **Projects** with member management
- **Task Assignments** to team members
- **Shared Links** for external collaboration

### ⏱️ Time Tracking
- **Time Entries** for task duration tracking
- **Start/Stop Timer** functionality
- **Time Reports** and analytics

### 🔔 Real-time Features
- **SignalR Integration** for live updates
- **Real-time Notifications** for task changes
- **Instant Collaboration** updates

### 📊 Analytics & Reporting
- **Dashboard Statistics** with comprehensive metrics
- **Completion Rate** tracking
- **Priority Distribution** analysis
- **Category Performance** insights
- **Time-based Analytics** (daily, weekly, monthly)

### 🔗 Integrations
- **Google Calendar Sync** for task scheduling
- **Email Notifications** with SMTP support
- **File Storage** with configurable providers

### 🛡️ Security & Performance
- **Rate Limiting** to prevent abuse
- **Global Exception Handling** with structured logging
- **Input Validation** with FluentValidation
- **SQL Injection Protection** with parameterized queries
- **XSS Protection** with input sanitization
- **CORS Configuration** for secure cross-origin requests
- **Database Indexing** for optimized queries
- **Async/Await** for non-blocking operations
- **Connection Pooling** for efficient database access

---

## 🛠️ Technology Stack

### Backend
- **Framework**: ASP.NET Core 8.0
- **Language**: C# 12.0
- **ORM**: Entity Framework Core 8.0
- **Database**: SQL Server 2022
- **Authentication**: ASP.NET Core Identity + JWT
- **Real-time**: SignalR
- **Validation**: FluentValidation
- **Logging**: Serilog
- **API Documentation**: Swagger/OpenAPI

### Architecture & Patterns
- **Clean Architecture** (Domain-Driven Design)
- **Repository Pattern** for data access abstraction
- **Dependency Injection** for loose coupling
- **CQRS Pattern** for complex operations
- **Unit of Work** for transaction management

### Security
- **JWT Tokens** for stateless authentication
- **BCrypt** for password hashing
- **Rate Limiting** with AspNetCoreRateLimit
- **HTTPS Enforcement** in production
- **CORS** with configurable policies

### DevOps & Tools
- **Version Control**: Git
- **API Testing**: Swagger UI, Postman
- **Database Migrations**: EF Core Migrations
- **Environment Configuration**: User Secrets, Environment Variables

---

## 🏗️ Architecture

The project follows **Clean Architecture** principles with clear separation of concerns:

```
TaskManagement/
├── TaskManagement.Core/           # Domain Layer
│   ├── Entities/                  # Domain entities
│   ├── Interfaces/                # Repository & service contracts
│   ├── Enums/                     # Domain enumerations
│   ├── Exceptions/                # Custom exceptions
│   └── Settings/                  # Configuration models
│
├── TaskManagement.Infrastructure/ # Data Access Layer
│   ├── Data/                      # DbContext & configurations
│   ├── Repositories/              # Repository implementations
│   └── Migrations/                # EF Core migrations
│
├── TaskManagement.Services/       # Business Logic Layer
│   └── Services/                  # Service implementations
│
└── TaskManagement.Api/            # Presentation Layer
    ├── Controllers/               # API endpoints
    ├── DTOs/                      # Data transfer objects
    ├── Validators/                # Input validation
    ├── Middleware/                # Custom middleware
    ├── Hubs/                      # SignalR hubs
    └── Services/                  # API-specific services
```

### Key Design Principles

1. **Separation of Concerns**: Each layer has a specific responsibility
2. **Dependency Inversion**: High-level modules don't depend on low-level modules
3. **Single Responsibility**: Each class has one reason to change
4. **Open/Closed Principle**: Open for extension, closed for modification
5. **Interface Segregation**: Clients shouldn't depend on unused interfaces

---

## 🚀 Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 8.0 SDK** or later ([Download](https://dotnet.microsoft.com/download))
- **SQL Server 2019+** or SQL Server Express ([Download](https://www.microsoft.com/sql-server/sql-server-downloads))
- **Visual Studio 2022** or **VS Code** with C# extension
- **Git** for version control

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/muhamedessamz/Zenith-Task-Management-API.git
   cd Zenith-Task-Management-API
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Navigate to the API project**
   ```bash
   cd TaskManagement.Api
   ```

### Configuration

#### 1. Database Connection String

Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TaskManagementDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

#### 2. JWT Settings

Configure JWT settings in `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyHere_MinimumLength32Characters!",
    "Issuer": "TaskManagementAPI",
    "Audience": "TaskManagementClient",
    "ExpiryInMinutes": 30
  }
}
```

> ⚠️ **Security Note**: Never commit sensitive keys to version control. Use **User Secrets** for development and **Environment Variables** for production.

#### 3. Email Settings (Optional)

For email notifications, configure SMTP settings:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderName": "Task Management System",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "EnableSsl": true
  }
}
```

#### 4. Using User Secrets (Recommended for Development)

```bash
# Initialize user secrets
dotnet user-secrets init

# Set database connection
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YourConnectionString"

# Set JWT secret
dotnet user-secrets set "JwtSettings:SecretKey" "YourSecretKey"

# Set email password
dotnet user-secrets set "EmailSettings:Password" "YourEmailPassword"
```

Or use the provided PowerShell script:

```powershell
.\setup-user-secrets.ps1
```

### Database Setup

1. **Apply migrations to create the database**
   ```bash
   dotnet ef database update --project ../TaskManagement.Infrastructure
   ```

2. **Verify database creation**
   - Open SQL Server Management Studio (SSMS)
   - Connect to your server
   - Verify `TaskManagementDb` database exists

### Running the Application

1. **Start the API**
   ```bash
   dotnet run
   ```

2. **Access Swagger UI**
   ```
   https://localhost:7287/swagger
   ```

3. **Health Check**
   ```
   https://localhost:7287/health
   ```

---

## 📚 API Documentation

### Base URL
```
https://localhost:7287/api
```

### Authentication Flow

#### 1. Register a New User
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "user": {
    "id": "c29dd89f-fd7f-4e18-82f9-ff6f7e0c6e9a",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe"
  }
}
```

#### 2. Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!"
}
```

#### 3. Using the Token

Add the token to all subsequent requests:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### Core Endpoints

#### Tasks

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/tasks` | Get all tasks | ✅ |
| GET | `/api/tasks/paged` | Get paginated tasks | ✅ |
| GET | `/api/tasks/{id}` | Get task by ID | ✅ |
| POST | `/api/tasks` | Create new task | ✅ |
| PUT | `/api/tasks/{id}` | Update task | ✅ |
| DELETE | `/api/tasks/{id}` | Delete task | ✅ |
| GET | `/api/tasks/search` | Search tasks | ✅ |
| GET | `/api/tasks/advanced-filter` | Advanced filtering | ✅ |
| GET | `/api/tasks/date-range` | Filter by date range | ✅ |

#### Categories

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/categories` | Get all categories | ✅ |
| GET | `/api/categories/{id}` | Get category by ID | ✅ |
| POST | `/api/categories` | Create category | ✅ |
| PUT | `/api/categories/{id}` | Update category | ✅ |
| DELETE | `/api/categories/{id}` | Delete category | ✅ |

#### Projects

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/projects` | Get all projects | ✅ |
| GET | `/api/projects/{id}` | Get project by ID | ✅ |
| POST | `/api/projects` | Create project | ✅ |
| PUT | `/api/projects/{id}` | Update project | ✅ |
| DELETE | `/api/projects/{id}` | Delete project | ✅ |
| POST | `/api/projects/{id}/members` | Add member | ✅ |
| DELETE | `/api/projects/{id}/members/{userId}` | Remove member | ✅ |

#### Comments

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/tasks/{taskId}/comments` | Get task comments | ✅ |
| POST | `/api/tasks/{taskId}/comments` | Add comment | ✅ |
| PUT | `/api/comments/{id}` | Update comment | ✅ |
| DELETE | `/api/comments/{id}` | Delete comment | ✅ |

#### Attachments

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/tasks/{taskId}/attachments` | Get attachments | ✅ |
| POST | `/api/tasks/{taskId}/attachments` | Upload attachment | ✅ |
| GET | `/api/attachments/{id}/download` | Download file | ✅ |
| DELETE | `/api/attachments/{id}` | Delete attachment | ✅ |

#### Dashboard

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/dashboard/stats` | Get dashboard statistics | ✅ |
| GET | `/api/dashboard/tasks-per-day` | Get daily task metrics | ✅ |

### Request/Response Examples

#### Create a Task
```http
POST /api/tasks
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Complete API Documentation",
  "description": "Write comprehensive documentation for all endpoints",
  "priority": 3,
  "dueDate": "2026-01-15T10:00:00Z",
  "categoryId": 1,
  "recurrencePattern": 0
}
```

**Response:**
```json
{
  "id": 42,
  "title": "Complete API Documentation",
  "description": "Write comprehensive documentation for all endpoints",
  "isCompleted": false,
  "priority": 3,
  "createdAt": "2026-01-01T00:00:00Z",
  "dueDate": "2026-01-15T10:00:00Z",
  "userId": "c29dd89f-fd7f-4e18-82f9-ff6f7e0c6e9a",
  "categoryId": 1,
  "categoryName": "Work"
}
```

#### Get Dashboard Statistics
```http
GET /api/dashboard/stats
Authorization: Bearer {token}
```

**Response:**
```json
{
  "totalTasks": 50,
  "completedTasks": 30,
  "pendingTasks": 20,
  "overdueTasks": 5,
  "completionRate": 60.00,
  "priorityStats": {
    "low": 10,
    "medium": 20,
    "high": 15,
    "critical": 5
  },
  "categoryStats": {
    "categories": [
      {
        "categoryId": 1,
        "categoryName": "Work",
        "color": "#3b82f6",
        "taskCount": 25,
        "completedCount": 15
      }
    ],
    "uncategorized": 10
  }
}
```

### Error Handling

All errors follow a consistent format:

```json
{
  "statusCode": 404,
  "message": "Task with key '999' was not found.",
  "errors": null,
  "timestamp": "2026-01-01T00:00:00Z",
  "path": "/api/tasks/999"
}
```

**Common Status Codes:**
- `200 OK` - Request successful
- `201 Created` - Resource created successfully
- `204 No Content` - Successful deletion
- `400 Bad Request` - Validation error
- `401 Unauthorized` - Authentication required
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `409 Conflict` - Duplicate resource
- `429 Too Many Requests` - Rate limit exceeded
- `500 Internal Server Error` - Server error

---

## 📁 Project Structure

```
TaskManagement.Core/
├── Entities/
│   ├── User.cs                    # User entity (ASP.NET Identity)
│   ├── TaskItem.cs                # Task entity
│   ├── Category.cs                # Category entity
│   ├── Tag.cs                     # Tag entity
│   ├── Project.cs                 # Project entity
│   ├── ProjectMember.cs           # Project membership
│   ├── Comment.cs                 # Task comments
│   ├── TaskAttachment.cs          # File attachments
│   ├── ChecklistItem.cs           # Checklist items
│   ├── TimeEntry.cs               # Time tracking
│   ├── TaskDependency.cs          # Task dependencies
│   ├── TaskAssignment.cs          # Task assignments
│   ├── SharedLink.cs              # Shared task links
│   ├── EmailOtp.cs                # Email verification OTPs
│   ├── RefreshToken.cs            # JWT refresh tokens
│   └── UserCalendarIntegration.cs # Calendar sync
│
├── Interfaces/
│   ├── ITaskRepository.cs
│   ├── ITaskService.cs
│   ├── ICategoryRepository.cs
│   ├── IProjectRepository.cs
│   ├── IEmailService.cs
│   ├── INotificationService.cs
│   ├── IFileService.cs
│   ├── ITimeTrackingService.cs
│   └── ... (20+ interfaces)
│
├── Enums/
│   ├── TaskPriority.cs            # Low, Medium, High, Critical
│   └── RecurrencePattern.cs       # None, Daily, Weekly, Monthly
│
├── Exceptions/
│   └── NotFoundException.cs       # Custom exceptions
│
└── Settings/
    ├── JwtSettings.cs             # JWT configuration
    └── EmailSettings.cs           # Email configuration

TaskManagement.Infrastructure/
├── Data/
│   ├── AppDbContext.cs            # EF Core DbContext
│   └── EntityConfigurations/      # Fluent API configurations
│
├── Repositories/
│   ├── TaskRepository.cs
│   ├── CategoryRepository.cs
│   ├── ProjectRepository.cs
│   ├── CommentRepository.cs
│   ├── AttachmentRepository.cs
│   └── ChecklistRepository.cs
│
└── Migrations/                    # EF Core migrations

TaskManagement.Api/
├── Controllers/
│   ├── AuthController.cs          # Authentication endpoints
│   ├── TasksController.cs         # Task CRUD operations
│   ├── CategoriesController.cs    # Category management
│   ├── TagsController.cs          # Tag management
│   ├── ProjectsController.cs      # Project management
│   ├── CommentsController.cs      # Comment operations
│   ├── AttachmentsController.cs   # File upload/download
│   ├── ChecklistController.cs     # Checklist management
│   ├── DashboardController.cs     # Analytics & stats
│   ├── UsersController.cs         # User profile management
│   ├── TimeTrackingController.cs  # Time tracking
│   ├── TaskDependenciesController.cs
│   ├── SharedLinksController.cs
│   └── CalendarController.cs
│
├── DTOs/
│   ├── Auth/                      # Authentication DTOs
│   ├── Task/                      # Task DTOs
│   ├── Category/                  # Category DTOs
│   ├── Project/                   # Project DTOs
│   └── ... (organized by feature)
│
├── Validators/
│   ├── TaskCreateDtoValidator.cs
│   ├── TaskUpdateDtoValidator.cs
│   ├── RegisterDtoValidator.cs
│   └── ... (FluentValidation validators)
│
├── Middleware/
│   └── GlobalExceptionHandlerMiddleware.cs
│
├── Hubs/
│   └── NotificationsHub.cs        # SignalR hub
│
├── Services/
│   ├── AuthService.cs
│   ├── EmailService.cs
│   ├── NotificationService.cs
│   ├── FileService.cs
│   ├── FileCleanupService.cs
│   ├── DashboardService.cs
│   ├── TimeTrackingService.cs
│   ├── TaskDependencyService.cs
│   ├── GoogleCalendarService.cs
│   ├── SharedLinkService.cs
│   ├── TaskAssignmentService.cs
│   └── OtpService.cs
│
└── Program.cs                     # Application entry point
```

---

## 🔒 Security

### Authentication & Authorization

- **JWT Tokens**: Stateless authentication with 30-minute expiration
- **Refresh Tokens**: Long-lived tokens for seamless session renewal
- **Password Hashing**: BCrypt with salt for secure password storage
- **Role-based Access**: Admin, User, and Manager roles
- **Email Verification**: OTP-based email confirmation

### Security Best Practices

1. **HTTPS Enforcement**: All production traffic uses HTTPS
2. **CORS Configuration**: Whitelist specific origins
3. **Rate Limiting**: Prevent brute-force attacks
   - 100 requests per 5 minutes per IP
   - Configurable per endpoint
4. **Input Validation**: FluentValidation for all inputs
5. **SQL Injection Protection**: Parameterized queries via EF Core
6. **XSS Protection**: Input sanitization and output encoding
7. **Secrets Management**: User Secrets (dev) + Environment Variables (prod)
8. **Token Storage**: HTTP-only cookies for refresh tokens

### Rate Limiting Configuration

```json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": false,
    "RealIpHeader": "X-Real-IP",
    "ClientIdHeader": "X-ClientId",
    "HttpStatusCode": 429,
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "5m",
        "Limit": 100
      }
    ]
  }
}
```

---

## ⚡ Performance Optimizations

### Database Optimizations

1. **Indexing Strategy**
   - Primary keys on all entities
   - Foreign key indexes
   - Composite indexes on frequently queried columns
   - Covering indexes for common queries

2. **Query Optimization**
   - `AsNoTracking()` for read-only queries
   - Eager loading with `.Include()` to prevent N+1 queries
   - Projection with `.Select()` to fetch only required fields
   - Pagination to limit result sets

3. **Connection Pooling**
   - EF Core connection pooling enabled
   - Optimized connection string parameters

### Application Optimizations

1. **Async/Await**: All I/O operations are asynchronous
2. **Caching**: In-memory caching for frequently accessed data
3. **Response Compression**: Gzip compression for API responses
4. **File Cleanup**: Background service for orphaned file deletion

### Monitoring & Logging

- **Serilog**: Structured logging with multiple sinks
- **Health Checks**: `/health` endpoint for monitoring
- **Performance Metrics**: Request duration logging
- **Error Tracking**: Detailed exception logging with stack traces

---

## 🧪 Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true
```

### Test Structure

```
TaskManagement.Tests/
├── Unit/
│   ├── Services/
│   ├── Repositories/
│   └── Validators/
│
├── Integration/
│   ├── Controllers/
│   └── Database/
│
└── E2E/
    └── Scenarios/
```

### Testing Tools

- **xUnit**: Testing framework
- **Moq**: Mocking framework
- **FluentAssertions**: Assertion library
- **WebApplicationFactory**: Integration testing

---

## 🚢 Deployment

### Prerequisites

- SQL Server database (Azure SQL, AWS RDS, or on-premises)
- SMTP server for email notifications
- SSL certificate for HTTPS

### Environment Variables

Set the following environment variables in production:

```bash
ConnectionStrings__DefaultConnection="YourProductionConnectionString"
JwtSettings__SecretKey="YourProductionSecretKey"
EmailSettings__Password="YourEmailPassword"
ASPNETCORE_ENVIRONMENT="Production"
```

### Deployment Options

#### 1. Azure App Service

```bash
# Publish to Azure
dotnet publish -c Release
az webapp up --name your-app-name --resource-group your-rg
```

#### 2. Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManagement.Api.dll"]
```

```bash
# Build and run
docker build -t task-management-api .
docker run -p 8080:80 task-management-api
```

#### 3. IIS

1. Publish the application: `dotnet publish -c Release`
2. Copy published files to IIS wwwroot
3. Configure application pool (.NET CLR Version: No Managed Code)
4. Set environment variables in web.config

### Database Migration in Production

```bash
# Generate SQL script
dotnet ef migrations script --output migration.sql --project TaskManagement.Infrastructure

# Apply manually or use automated deployment
```

---

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

### Development Workflow

1. **Fork the repository**
2. **Create a feature branch**
   ```bash
   git checkout -b feature/amazing-feature
   ```
3. **Commit your changes**
   ```bash
   git commit -m "Add amazing feature"
   ```
4. **Push to the branch**
   ```bash
   git push origin feature/amazing-feature
   ```
5. **Open a Pull Request**

### Code Standards

- Follow C# coding conventions
- Write unit tests for new features
- Update documentation for API changes
- Use meaningful commit messages
- Ensure all tests pass before submitting PR

### Reporting Issues

- Use GitHub Issues for bug reports
- Provide detailed reproduction steps
- Include error messages and stack traces
- Specify environment details (OS, .NET version, etc.)

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## 👨‍💻 Author

**Mohamed Essam**
- GitHub: [@muhamedessamz](https://github.com/muhamedessamz)
- LinkedIn: [Mohamed Essam](https://www.linkedin.com/in/mohamedessamz/)

---

## 🙏 Acknowledgments

- ASP.NET Core team for the excellent framework
- Entity Framework Core for seamless data access
- FluentValidation for elegant validation
- Serilog for structured logging
- SignalR for real-time communication

---

## 📞 Support

For support, please:
- Open an issue on GitHub
- Check existing documentation
- Review closed issues for solutions

---

<div align="center">

**⭐ Star this repository if you find it helpful!**

Made with ❤️ by Mohamed Essam

</div>
