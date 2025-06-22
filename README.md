# BookLanding - Online Bookstore

A modern ASP.NET Core MVC application for online book sales with comprehensive user management, authentication, and admin features.

## 🚀 Features

- **User Authentication & Authorization**
  - JWT-based authentication
  - Google OAuth integration
  - Password reset functionality
  - User registration with email verification

- **Admin Panel**
  - User management
  - Administrative dashboard
  - Role-based access control

- **Security Features**
  - Secure password handling
  - CAPTCHA integration
  - Session management
  - Cryptographic services

- **Modern UI**
  - Responsive design with Bootstrap
  - AdminLTE admin interface
  - BookLanding custom theme

## 🔧 Technology Stack

- **Backend**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server with Dapper ORM
- **Authentication**: JWT + Cookie Authentication
- **Frontend**: Bootstrap, jQuery, AdminLTE
- **Logging**: Serilog
- **Email**: Gmail SMTP integration

## ⚠️ SECURITY SETUP REQUIRED

**IMPORTANT**: This repository does not contain sensitive configuration data. You must configure secrets before running the application.

### Quick Start

1. **Clone the repository**
   ```bash
   git clone <your-repo-url>
   cd booklanding
   ```

2. **Configure secrets** (Choose one method)

   **Option A: Using ASP.NET Core User Secrets (Recommended for Development)**
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "JwtSettings:SignKey" "your-32-character-secret-key"
   dotnet user-secrets set "ConnectionStrings:DbConn" "your-database-connection-string"
   # Add other required secrets (see SECURITY.md)
   ```

   **Option B: Using Configuration Files**
   ```bash
   cp appsettings.template.json appsettings.json
   cp appsettings.Development.template.json appsettings.Development.json
   # Edit the files and replace {PLACEHOLDER} values with actual secrets
   ```

3. **Install dependencies**
   ```bash
   dotnet restore
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the application**
   - Main site: https://localhost:7890
   - Admin panel: https://localhost:7890/Admin

## 📋 Required Configuration

Before running the application, you need to configure:

- **Database Connection**: SQL Server connection string
- **JWT Settings**: Signing key for token generation
- **Email Settings**: Gmail SMTP configuration
- **Google OAuth**: Client ID and Secret for social login

**📖 For detailed setup instructions, see [SECURITY.md](SECURITY.md)**

## 🏗️ Project Structure

```
├── AppCodes/
│   ├── AppBase/          # Base classes and repositories
│   ├── AppConfig/        # Configuration classes
│   ├── AppInterface/     # Service interfaces
│   ├── AppMiddleware/    # Custom middleware
│   ├── AppRepo/          # Data repositories
│   └── AppService/       # Business logic services
├── Areas/
│   └── Admin/            # Admin area with controllers and views
├── Controllers/          # MVC controllers
├── Data/                 # Database scripts and logs
├── Models/               # Data models and view models
├── Views/                # Razor views and layouts
├── wwwroot/              # Static files (CSS, JS, images)
└── Properties/           # Launch settings
```

## 🔐 Security Features

- **Authentication**: Multi-layer authentication with JWT and cookies
- **Authorization**: Role-based access control
- **Password Security**: Secure hashing and validation
- **CAPTCHA**: Bot protection
- **Email Verification**: Account activation via email
- **Session Management**: Secure session handling
- **CSRF Protection**: Cross-site request forgery protection

## 🚀 Deployment

### Development
```bash
dotnet run --environment Development
```

### Production
1. Configure production environment variables
2. Update connection strings for production database
3. Enable HTTPS and security headers
4. Configure proper logging levels

See [SECURITY.md](SECURITY.md) for detailed deployment instructions.

## 📚 API Documentation

When running in development mode with Swagger enabled, visit:
- Swagger UI: https://localhost:7890/swagger

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Ensure all tests pass
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🔒 Security

For security-related issues, please review our [Security Guide](SECURITY.md) and report vulnerabilities responsibly.

## 📞 Support

For support and questions:
- Create an issue in the GitHub repository
- Review the documentation in [SECURITY.md](SECURITY.md)
- Check the project wiki for additional resources

---

**⚠️ Remember**: Never commit sensitive configuration data to version control. Always use environment variables or user secrets for sensitive information.
