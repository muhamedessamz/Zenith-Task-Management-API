# 🚀 Deployment Guide - Task Management System

## 📋 Table of Contents
- [Before You Deploy](#before-you-deploy)
- [Local Development Setup](#local-development-setup)
- [Production Deployment](#production-deployment)
- [Environment Variables](#environment-variables)
- [Security Checklist](#security-checklist)

---

## 🔐 Before You Deploy

### ⚠️ IMPORTANT: Configure Secrets First!

This repository contains **placeholder values** in `appsettings.json`. You **MUST** configure real secrets before running the application.

### Required Secrets:
1. **Database Connection String**
2. **JWT Secret Key** (minimum 32 characters)
3. **Email SMTP Password** (Gmail App Password)
4. **Google Calendar OAuth Credentials** (optional)

---

## 💻 Local Development Setup

### Option 1: Using User Secrets (Recommended) ⭐

User Secrets keep your sensitive data **outside** the project directory and **never** get committed to Git.

#### Step 1: Initialize User Secrets
```bash
cd TaskManagement.Api
dotnet user-secrets init
```

#### Step 2: Set Your Secrets
```bash
# Database Connection
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=TaskManagementDb;Trusted_Connection=True;TrustServerCertificate=True;"

# JWT Settings
dotnet user-secrets set "JwtSettings:SecretKey" "YourSuperSecretKeyMinimum32CharactersLong!@#$%"

# Email Settings
dotnet user-secrets set "EmailSettings:SenderEmail" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:Password" "your-gmail-app-password"

# Google Calendar (Optional)
dotnet user-secrets set "GoogleCalendar:ClientId" "your-google-client-id"
dotnet user-secrets set "GoogleCalendar:ClientSecret" "your-google-client-secret"
```

#### Step 3: Or Use the PowerShell Script
```powershell
.\setup-user-secrets.ps1
```

---

### Option 2: Using appsettings.Development.json

Create `TaskManagement.Api/appsettings.Development.json` (this file is **gitignored**):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TaskManagementDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyMinimum32CharactersLong!@#$%"
  },
  "EmailSettings": {
    "SenderEmail": "your-email@gmail.com",
    "Password": "your-gmail-app-password"
  },
  "GoogleCalendar": {
    "ClientId": "your-google-client-id",
    "ClientSecret": "your-google-client-secret"
  }
}
```

---

## 🌐 Production Deployment

### Option 1: Azure App Service

#### 1. Create Azure SQL Database
```bash
az sql server create --name your-server-name --resource-group your-rg --location eastus --admin-user sqladmin --admin-password YourPassword123!
az sql db create --resource-group your-rg --server your-server-name --name TaskManagementDb --service-objective S0
```

#### 2. Configure Application Settings
Go to Azure Portal → App Service → Configuration → Application Settings:

```
ConnectionStrings__DefaultConnection = Server=your-server.database.windows.net;Database=TaskManagementDb;User Id=sqladmin;Password=YourPassword123!;
JwtSettings__SecretKey = YourProductionSecretKey32CharactersMin!
EmailSettings__SenderEmail = your-production-email@gmail.com
EmailSettings__Password = your-production-app-password
```

#### 3. Deploy
```bash
dotnet publish -c Release
az webapp up --name your-app-name --resource-group your-rg
```

---

### Option 2: Docker

#### 1. Build Docker Image
```bash
docker build -t task-management-api .
```

#### 2. Run with Environment Variables
```bash
docker run -d -p 8080:80 \
  -e "ConnectionStrings__DefaultConnection=Server=your-server;Database=TaskManagementDb;User Id=sa;Password=YourPassword123!;" \
  -e "JwtSettings__SecretKey=YourProductionSecretKey32CharactersMin!" \
  -e "EmailSettings__SenderEmail=your-email@gmail.com" \
  -e "EmailSettings__Password=your-app-password" \
  --name task-api \
  task-management-api
```

#### 3. Or Use Docker Compose
Create `docker-compose.yml`:

```yaml
version: '3.8'
services:
  api:
    build: .
    ports:
      - "8080:80"
    environment:
      - ConnectionStrings__DefaultConnection=Server=db;Database=TaskManagementDb;User Id=sa;Password=YourPassword123!;
      - JwtSettings__SecretKey=YourProductionSecretKey32CharactersMin!
      - EmailSettings__SenderEmail=your-email@gmail.com
      - EmailSettings__Password=your-app-password
    depends_on:
      - db
  
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword123!
    ports:
      - "1433:1433"
```

Run:
```bash
docker-compose up -d
```

---

### Option 3: IIS (Windows Server)

#### 1. Publish Application
```bash
dotnet publish -c Release -o C:\inetpub\wwwroot\TaskManagementAPI
```

#### 2. Create appsettings.Production.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=TaskManagementDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "YourProductionSecretKey32CharactersMin!"
  },
  "EmailSettings": {
    "SenderEmail": "your-production-email@gmail.com",
    "Password": "your-production-app-password"
  }
}
```

#### 3. Configure IIS
1. Create new Application Pool (.NET CLR Version: No Managed Code)
2. Create new Website pointing to publish folder
3. Set environment variable: `ASPNETCORE_ENVIRONMENT=Production`

---

## 🔑 Environment Variables

### Required Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | Database connection string | `Server=localhost;Database=TaskManagementDb;...` |
| `JwtSettings__SecretKey` | JWT signing key (min 32 chars) | `YourSuperSecretKey32CharsMin!` |
| `EmailSettings__SenderEmail` | SMTP sender email | `your-email@gmail.com` |
| `EmailSettings__Password` | SMTP password (App Password) | `abcd efgh ijkl mnop` |

### Optional Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `JwtSettings__ExpirationMinutes` | Token expiration time | `30` |
| `EmailSettings__SmtpServer` | SMTP server address | `smtp.gmail.com` |
| `EmailSettings__SmtpPort` | SMTP port | `587` |
| `GoogleCalendar__ClientId` | Google OAuth Client ID | - |
| `GoogleCalendar__ClientSecret` | Google OAuth Client Secret | - |

---

## 🔒 Security Checklist

Before deploying to production:

- [ ] ✅ All secrets are configured (not using placeholders)
- [ ] ✅ JWT SecretKey is at least 32 characters long
- [ ] ✅ Using different secrets for Development and Production
- [ ] ✅ Database user has minimum required permissions
- [ ] ✅ HTTPS is enabled (SSL certificate configured)
- [ ] ✅ CORS is configured with specific origins (not "*")
- [ ] ✅ Rate limiting is enabled
- [ ] ✅ Email uses App Password (not actual Gmail password)
- [ ] ✅ Logs directory has write permissions
- [ ] ✅ Connection strings use encrypted connections
- [ ] ✅ appsettings.Development.json is NOT deployed to production
- [ ] ✅ Environment variable `ASPNETCORE_ENVIRONMENT` is set to "Production"

---

## 📧 Getting Gmail App Password

1. Go to [Google Account Settings](https://myaccount.google.com/)
2. Navigate to **Security** → **2-Step Verification** (enable if not already)
3. Scroll down to **App Passwords**
4. Select **Mail** and **Other (Custom name)**
5. Enter "Task Management API"
6. Click **Generate**
7. Copy the 16-character password (format: `xxxx xxxx xxxx xxxx`)
8. Use this password in `EmailSettings:Password`

---

## 🗄️ Database Migration

### Development
```bash
dotnet ef database update --project TaskManagement.Infrastructure
```

### Production (Generate SQL Script)
```bash
dotnet ef migrations script --output migration.sql --project TaskManagement.Infrastructure
```

Then run the SQL script on your production database.

---

## 🧪 Testing the Deployment

1. **Health Check**
   ```
   GET https://your-domain.com/health
   ```

2. **Swagger UI**
   ```
   https://your-domain.com/swagger
   ```

3. **Register Test User**
   ```bash
   curl -X POST https://your-domain.com/api/auth/register \
     -H "Content-Type: application/json" \
     -d '{"email":"test@example.com","password":"Test123!","firstName":"Test","lastName":"User"}'
   ```

---

## 📞 Support

If you encounter issues:
1. Check logs in `Logs/` directory
2. Verify all environment variables are set correctly
3. Ensure database connection is working
4. Check firewall rules for database access

---

## 🔄 Updating Secrets

To rotate secrets (recommended every 90 days):

1. Generate new JWT secret key
2. Generate new Gmail App Password
3. Update all configuration sources
4. Restart the application

---

<div align="center">

**🎉 Your application is now ready for deployment!**

For more information, see the main [README.md](README.md)

</div>
