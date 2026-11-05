# AcxiomCRM — Enterprise Sales & CRM Platform

An enterprise-grade Customer Relationship Management system built using ASP.NET Core 8.0 MVC, Entity Framework Core, ASP.NET Core Identity, Bootstrap 5, and Chart.js.

---

## Technical Overview

- **Framework:** ASP.NET Core 8.0 MVC & Web API (with cross-runtime roll-forward support)
- **Database:** SQLite (`AcxiomCRM.db`) with Entity Framework Core 8.0
- **Security:** ASP.NET Core Identity (PBKDF2 adaptive password hashing, account lockout, anti-forgery tokens)
- **Authorization:** Role-Based Access Control (`Admin`, `Manager`, `SalesExecutive`)
- **Frontend:** Bootstrap 5.3, Bootstrap Icons, jQuery Validation & Unobtrusive Validation, Chart.js 4.4
- **Architecture:** Layered Architecture (Presentation &rarr; Application &rarr; Domain/Business &rarr; Data Access &rarr; Database)

---

## Getting Started

### Prerequisites
- [.NET SDK 8.0 or higher](https://dotnet.microsoft.com/download)

### Run the Project
```bash
cd AcxiomCRM
dotnet run
```
Navigate to:
- **`http://localhost:5001`** (or `https://localhost:7001`)

The database will be automatically created and populated with demo data on first launch.

---

## Seeded Test Accounts

| Role | Username / Email | Password | Primary Permissions |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@acxiomcrm.com` | `Admin@123` | Full enterprise administration, User & Role management, Audit Logs, CRM records, Reports |
| **Manager** | `manager@acxiomcrm.com` | `Manager@123` | Pipeline monitoring, team lead & opportunity tracking, executive reports |
| **Sales Executive** | `sales@acxiomcrm.com` | `Sales@123` | Assigned customers, leads, opportunities, follow-up scheduling, activity logging |

---

## Project Structure

```
AcxiomCRM/
├── Controllers/              # MVC Controllers (Account, Dashboard, Customer, Lead, Opportunity, FollowUp, Activity, UserManagement, AuditLog, Report)
│   └── Api/                  # Secured REST API Controllers (Customers, Leads, Opportunities, Reports)
├── Data/                     # ApplicationDbContext & DbInitializer
├── DTOs/                     # API Data Transfer Objects
├── Models/                   # Domain Entities & Identity Models
├── Services/                 # Business Logic Interfaces & Implementations
├── ViewModels/               # Strongly-Typed ViewModels with Validation Rules
├── Views/                    # Razor Views organized by functional module
│   ├── Account/              # Login, Register, AccessDenied
│   ├── Activity/             # Activity Logging (Call, Meeting, Email, Task)
│   ├── AuditLog/             # System Audit Trail (Admin only)
│   ├── Customer/             # Customer CRUD & Master Records
│   ├── Dashboard/            # Analytics KPI Cards & Chart.js Visualizations
│   ├── FollowUp/             # Scheduled Follow-ups (Upcoming & Overdue)
│   ├── Lead/                 # Lead Capture & Qualification Workflow
│   ├── Opportunity/          # Deal Pipeline & Weighted Value Calculation
│   ├── Report/               # Customer, Lead, FollowUp, Opportunity, Pipeline Reports
│   └── UserManagement/       # User & Role Administration (Admin only)
└── wwwroot/                  # Static styles and scripts
```

---

## Core Capabilities & Implemented Rules

1. **Multi-Tier Validation:**
   - Client-side validation using unobtrusive jQuery validation.
   - Server-side model validation on all POST requests.
   - Business validations:
     - `Opportunity Amount > 0`
     - `Opportunity Probability` between `0` and `100`
     - `Expected Close Date` cannot be in the past for active deals
     - `Follow-Up Date` cannot be earlier than today for planned tasks
     - Customer email and phone uniqueness verification
     - Strict 10-digit Indian mobile format validation (`^[6-9]\d{9}$`)
     - Lead lifecycle status transition rules

2. **Security & Governance:**
   - Identity password hashing; no plain-text password storage.
   - Brute-force account lockout: 5 failed attempts locks the account for 15 minutes.
   - State-changing MVC forms protected with `[ValidateAntiForgeryToken]`.
   - Append-only audit logging tracking logins, failed attempts, lockouts, and CRM record changes.

3. **RESTful APIs:**
   - Exposes secured endpoints for Customers, Leads, Opportunities, and Pipeline Reports.
   - Returns standard HTTP response codes (`200`, `201`, `400`, `401`, `403`, `404`, `409`) and uses DTOs.

---

## Documentation

For the full specification breakdown, architectural decisions, and acceptance verification matrix, see:
- [PROJECT_SUBMISSION_REPORT.md](PROJECT_SUBMISSION_REPORT.md)
- [AcxiomCRM/PROJECT_DOCUMENTATION.md](AcxiomCRM/PROJECT_DOCUMENTATION.md)
