# Security Setup Guide

This document provides instructions for securely configuring the BookLanding ASP.NET Core application.

## ⚠️ IMPORTANT SECURITY NOTICE

**DO NOT commit the actual `appsettings.json` and `appsettings.Development.json` files to version control.** These files contain sensitive information and are excluded by `.gitignore`.

## 🔧 Initial Setup

### 1. Configuration Files Setup

Copy the template files and configure them with your actual values:

```bash
# Copy template files to create your local configuration
cp appsettings.template.json appsettings.json
cp appsettings.Development.template.json appsettings.Development.json
```

### 2. Required Environment Variables / Secrets

Replace the following placeholders in your local configuration files:

#### Database Configuration
- `{DB_SERVER}` - Your database server address
- `{DB_NAME}` - Your database name
- `{DB_USER}` - Database username
- `{DB_PASSWORD}` - Database password
- `{PROD_DB_SERVER}` - Production database server
- `{PROD_DB_NAME}` - Production database name
- `{PROD_DB_USER}` - Production database username
- `{PROD_DB_PASSWORD}` - Production database password

#### JWT Configuration
- `{JWT_SIGN_KEY}` - JWT signing key (minimum 32 characters, use a strong random string)

#### Email Configuration (Gmail)
- `{GMAIL_USERNAME}` - Gmail username
- `{GMAIL_APP_PASSWORD}` - Gmail app-specific password
- `{GMAIL_SENDER_EMAIL}` - Sender email address

#### Google OAuth Configuration
- `{GOOGLE_CLIENT_ID}` - Google OAuth Client ID
- `{GOOGLE_CLIENT_SECRET}` - Google OAuth Client Secret

## 🔐 Development Environment Setup

### Option 1: ASP.NET Core User Secrets (Recommended for Development)

Initialize and configure user secrets:

```bash
# Initialize user secrets
dotnet user-secrets init

# Set JWT signing key
dotnet user-secrets set "JwtSettings:SignKey" "your-super-secret-jwt-key-at-least-32-characters-long"

# Set database connection
dotnet user-secrets set "ConnectionStrings:DbConn" "Server=localhost;Database=BookLanding;User Id=sa;Password=YourPassword;TrustServerCertificate=True;Connection Timeout=120"

# Set Gmail configuration
dotnet user-secrets set "Gmail:UserName" "your-email@gmail.com"
dotnet user-secrets set "Gmail:AppPassword" "your-app-password"
dotnet user-secrets set "Gmail:SenderEmail" "your-email@gmail.com"

# Set Google OAuth
dotnet user-secrets set "Authentication:Google:ClientId" "your-google-client-id"
dotnet user-secrets set "Authentication:Google:ClientSecret" "your-google-client-secret"
```

### Option 2: Environment Variables

Set the following environment variables:

```bash
export JWT__SIGNKEY="your-super-secret-jwt-key-at-least-32-characters-long"
export CONNECTIONSTRINGS__DBCONN="Server=localhost;Database=BookLanding;User Id=sa;Password=YourPassword;TrustServerCertificate=True;Connection Timeout=120"
export GMAIL__USERNAME="your-email@gmail.com"
export GMAIL__APPPASSWORD="your-app-password"
export GMAIL__SENDEREMAIL="your-email@gmail.com"
export AUTHENTICATION__GOOGLE__CLIENTID="your-google-client-id"
export AUTHENTICATION__GOOGLE__CLIENTSECRET="your-google-client-secret"
```

## 🚀 Production Deployment

### Environment Variables for Production

Configure the following environment variables in your production environment:

```
JWT__SIGNKEY=<strong-random-key-minimum-32-chars>
CONNECTIONSTRINGS__DBCONN=<production-database-connection-string>
CONNECTIONSTRINGS__PRODUCTIONDBCONN=<production-database-connection-string>
GMAIL__USERNAME=<production-email-username>
GMAIL__APPPASSWORD=<production-email-app-password>
GMAIL__SENDEREMAIL=<production-sender-email>
AUTHENTICATION__GOOGLE__CLIENTID=<production-google-client-id>
AUTHENTICATION__GOOGLE__CLIENTSECRET=<production-google-client-secret>
```

### Azure App Service Configuration

If deploying to Azure App Service, add these as Application Settings:

1. Go to Azure Portal → App Service → Configuration
2. Add each environment variable as an Application Setting
3. Use double underscores (`__`) for nested configuration (e.g., `JWT__SIGNKEY`)

### Docker Deployment

For Docker deployments, use environment variables or Docker secrets:

```dockerfile
# Example environment variables in docker-compose.yml
environment:
  - JWT__SIGNKEY=your-secret-key
  - CONNECTIONSTRINGS__DBCONN=your-connection-string
  # ... other variables
```

## 🔒 Security Best Practices

### JWT Signing Key
- Use a cryptographically secure random string
- Minimum 32 characters
- Include uppercase, lowercase, numbers, and special characters
- Never reuse keys across environments

### Database Security
- Use strong passwords
- Enable SSL/TLS connections
- Use least-privilege database accounts
- Regularly rotate credentials

### Email Security
- Use app-specific passwords for Gmail
- Enable 2FA on email accounts
- Monitor for suspicious activity

### OAuth Security
- Use separate OAuth applications for different environments
- Regularly review OAuth permissions
- Monitor OAuth usage logs

## 📁 Files Excluded from Version Control

The following files are automatically excluded by `.gitignore`:

- `appsettings.json`
- `appsettings.Development.json`
- `appsettings.Production.json`
- `Properties/launchSettings.json`
- `Data/*.sql`
- `Data/logs/`
- All log files (`*.log`)

## 🚨 Security Checklist

Before deploying:

- [ ] All secrets removed from configuration files
- [ ] Environment variables configured
- [ ] Strong JWT signing key generated
- [ ] Database credentials secured
- [ ] OAuth credentials configured
- [ ] SSL/HTTPS enabled
- [ ] Log files excluded from version control
- [ ] Security headers configured
- [ ] CORS properly configured
- [ ] Input validation implemented

## 📞 Security Issues

If you discover a security vulnerability, please report it responsibly:

1. Do not create a public GitHub issue
2. Contact the development team directly
3. Provide detailed information about the vulnerability
4. Allow time for the issue to be addressed before public disclosure

## 🔄 Regular Security Maintenance

- Rotate secrets regularly (quarterly recommended)
- Update dependencies regularly
- Monitor security advisories
- Review access logs
- Audit user permissions
- Update SSL certificates before expiration
