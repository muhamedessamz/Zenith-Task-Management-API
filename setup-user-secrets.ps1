# Setup User Secrets for Development
# Run this script once to configure your local development environment
# ⚠️ IMPORTANT: Replace all placeholder values with your actual secrets before running!

Write-Host "🔐 Setting up User Secrets for Development..." -ForegroundColor Cyan
Write-Host "⚠️  Make sure to replace placeholder values with your actual secrets!" -ForegroundColor Yellow

# Navigate to the API project
Set-Location -Path ".\TaskManagement.Api"

# Initialize user secrets (if not already initialized)
Write-Host "`nInitializing User Secrets..." -ForegroundColor Yellow
dotnet user-secrets init

# Set Database Connection String
Write-Host "`nSetting Database Connection String..." -ForegroundColor Yellow
Write-Host "📝 Update YOUR_SERVER_NAME with your SQL Server instance name" -ForegroundColor Cyan
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER_NAME;Database=TaskManagementDb;Trusted_Connection=True;TrustServerCertificate=True;"

# Set JWT Settings
Write-Host "`nSetting JWT Secret Key..." -ForegroundColor Yellow
Write-Host "📝 Replace with a strong secret key (minimum 32 characters)" -ForegroundColor Cyan
dotnet user-secrets set "JwtSettings:SecretKey" "YOUR_JWT_SECRET_KEY_MIN_32_CHARS_REPLACE_THIS!"

# Set Email Settings
Write-Host "`nSetting Email Configuration..." -ForegroundColor Yellow
Write-Host "📝 Use your Gmail address and App Password (not your regular password)" -ForegroundColor Cyan
Write-Host "📧 Get App Password from: https://myaccount.google.com/apppasswords" -ForegroundColor Cyan
dotnet user-secrets set "EmailSettings:SenderEmail" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:Password" "your-gmail-app-password-here"

# Set Google Calendar Settings (Optional)
Write-Host "`nSetting Google Calendar API Credentials (Optional)..." -ForegroundColor Yellow
Write-Host "📝 Get credentials from: https://console.cloud.google.com/" -ForegroundColor Cyan
dotnet user-secrets set "GoogleCalendar:ClientId" "your-google-client-id-here"
dotnet user-secrets set "GoogleCalendar:ClientSecret" "your-google-client-secret-here"

# List all secrets to verify
Write-Host "`n✅ User Secrets configured successfully!" -ForegroundColor Green
Write-Host "`nConfigured secrets:" -ForegroundColor Cyan
dotnet user-secrets list

Write-Host "`n⚠️  REMINDER: Make sure you replaced all placeholder values!" -ForegroundColor Yellow
Write-Host "`n🚀 You can now run the application with: dotnet run" -ForegroundColor Green
Write-Host "📝 Swagger will be available at: http://localhost:5287/swagger" -ForegroundColor Cyan

# Return to root directory
Set-Location -Path ".."
