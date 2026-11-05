# AcxiomCRM

Complete Enterprise Customer Relationship Management System developed in ASP.NET Core 8.0 MVC with Entity Framework Core, ASP.NET Core Identity, Bootstrap 5, Chart.js, and REST APIs.

## Solution Structure
- `AcxiomCRM.sln` : Visual Studio solution file
- `AcxiomCRM/` : ASP.NET Core Web Application
  - `Controllers/` : MVC Controllers (Account, Dashboard, Customer, Lead, Opportunity, FollowUp, Activity, UserManagement, AuditLog, Report, Home)
  - `Controllers/Api/` : REST API Controllers (Customers, Leads, Opportunities, Reports)
  - `Models/` : Domain Entities (ApplicationUser, ApplicationRole, Customer, Lead, Opportunity, FollowUp, Activity, AuditLog)
  - `Data/` : ApplicationDbContext and DbInitializer
  - `Services/` : Business Logic & Interface Contracts (Audit, Customer, Lead, Opportunity, FollowUp, Activity, Dashboard, Report)
  - `ViewModels/` : Strongly-typed MVC ViewModels with validation attributes
  - `DTOs/` : Data Transfer Objects for REST APIs
  - `Views/` : Razor Views organized by module with Bootstrap 5 and Chart.js
  - `wwwroot/` : Static files (css, js)

## Pre-Configured Test Credentials
- **Admin**: `admin@acxiomcrm.com` / `Admin@123`
- **Manager**: `manager@acxiomcrm.com` / `Manager@123`
- **Sales Executive**: `sales@acxiomcrm.com` / `Sales@123`

## Quick Start
```bash
cd AcxiomCRM
dotnet run
```
Open `https://localhost:7001` or `http://localhost:5001`.

For complete project documentation, functional specifications, validation rules, security specifications, and test acceptance criteria, please see `AcxiomCRM/PROJECT_DOCUMENTATION.md`.

