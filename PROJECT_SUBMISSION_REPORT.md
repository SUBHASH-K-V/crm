# AcxiomCRM — Technical Assessment Submission Report

**Project Name:** AcxiomCRM  
**Submission Type:** Technical Assessment & Project Delivery Baseline  
**Architecture:** Layered Enterprise Architecture (Presentation, Application, Domain/Business, Data Access, Persistence)  
**Primary Framework:** ASP.NET Core MVC & Web API  
**Database:** Entity Framework Core with SQLite (`AcxiomCRM.db`)  
**Security Provider:** ASP.NET Core Identity with PBKDF2 Password Hashing & Role-Based Access Control  

---

## 1. Executive Summary

This project delivers **AcxiomCRM**, an enterprise-grade Customer Relationship Management system designed to manage the end-to-end sales lifecycle—from lead capture and customer master data to opportunity pipeline tracking, follow-up scheduling, activity logging, and executive reporting.

The solution was engineered following clean software architecture principles, separating concerns across presentation, service coordination, domain validation, data access, and cross-cutting security/audit services. All functional and technical requirements specified in the project baseline have been implemented and verified.

---

## 2. Technology Stack & Key Libraries

| Component | Technology | Version | Purpose |
| :--- | :--- | :--- | :--- |
| **Framework** | ASP.NET Core | 8.0 LTS (with Multi-Runtime RollForward) | Core Web Application & REST APIs |
| **ORM** | Entity Framework Core | 8.0.11 | Data modeling, migrations, LINQ queries |
| **Database** | SQLite | 3.x | Lightweight, zero-configuration local database |
| **Identity & Auth** | ASP.NET Core Identity | 8.0.11 | Authentication, lockout, cookie sessions, RBAC |
| **Styling & UI** | Bootstrap & Bootstrap Icons | 5.3.2 / 1.11.3 | Responsive modern interface, accessible components |
| **Data Visualization** | Chart.js | 4.4.1 | Interactive client-side pipeline and sales charts |
| **Client Validation** | jQuery Validation + Unobtrusive | 1.19.5 / 4.0.0 | Client-side immediate form feedback |

---

## 3. Layered Architecture & Design Patterns

The application follows strict layer separation:

```
┌─────────────────────────────────────────────────────────────────┐
│                       Presentation Layer                        │
│   Razor Views, Partial Views, TagHelpers, Chart.js, Bootstrap    │
└────────────────────────────────┬────────────────────────────────┘
                                 │
┌────────────────────────────────▼────────────────────────────────┐
│                       Application Layer                         │
│  MVC Controllers, API Controllers, ViewModels, DTO Data Models  │
└────────────────────────────────┬────────────────────────────────┘
                                 │
┌────────────────────────────────▼────────────────────────────────┐
│                     Domain / Service Layer                      │
│ Business Logic, Transition Validations, Scoped Calculations     │
└────────────────────────────────┬────────────────────────────────┘
                                 │
┌────────────────────────────────▼────────────────────────────────┐
│                       Data Access Layer                         │
│   ApplicationDbContext, Entity Configurations, EF Core Queries  │
└────────────────────────────────┬────────────────────────────────┘
                                 │
┌────────────────────────────────▼────────────────────────────────┐
│                        Database Layer                           │
│     SQLite Relational Store with Indexes & Foreign Keys         │
└─────────────────────────────────────────────────────────────────┘
 ─────────────────────────────────────────────────────────────────
 Cross-Cutting Services: ASP.NET Identity, Anti-Forgery, AuditLog
```

### Applied Design Patterns
1. **Service / Business Logic Separation**: Controllers remain lean, delegating CRM logic, scoping, and data mutations to scoped service interfaces (`ICustomerService`, `ILeadService`, `IOpportunityService`, `IFollowUpService`, `IActivityService`, `IDashboardService`, `IReportService`, `IAuditService`).
2. **Data Transfer Object (DTO) Pattern**: REST APIs strictly expose DTOs rather than internal Entity Framework Core entities, shielding sensitive database fields and preventing over-posting vulnerabilities.
3. **Role-Based Scoping Pattern**: Services dynamically inspect caller identity and role (`Admin`, `Manager`, `SalesExecutive`) to enforce record visibility boundaries at the database query level.
4. **Append-Oriented Audit Pattern**: Security and business changes are captured automatically via `IAuditService` into an immutable `AuditLog` table capturing actor, action, timestamp, entity, before/after values, and client IP.

---

## 4. Security & Authentication Implementation

### 4.1 Authentication & Session Management
- Integrated **ASP.NET Core Identity** utilizing secure PBKDF2 password hashing. Plain-text passwords are never persisted or logged.
- Cookie-based authentication configured with `SlidingExpiration`, 2-hour timeout, and secure redirect endpoints (`/Account/Login`, `/Account/AccessDenied`).
- State-changing MVC POST requests are protected with `[ValidateAntiForgeryToken]` to eliminate Cross-Site Request Forgery (CSRF).

### 4.2 Password Policy & Brute-Force Lockout
- **Password Policy:** Minimum length of 8 characters, requiring at least one uppercase letter, one lowercase letter, one numeric digit, and one special symbol.
- **Account Lockout:** Configured with `MaxFailedAccessAttempts = 5` and `DefaultLockoutTimeSpan = 15 minutes`.
- When an account exceeds failed attempts, the account is locked and an audit event (`AccountLockedOut`) is logged.
- Administrators can view lockout status and execute a one-click unlock from the User Management console.

### 4.3 Role-Based Authorization Matrix

| Module / Area | Admin | Manager | Sales Executive |
| :--- | :---: | :---: | :---: |
| **Dashboard** | Full Enterprise View | Team / Pipeline View | Personal Assigned Records |
| **Customers** | All Records (CRUD) | Team Scope (CRUD) | Assigned Customers |
| **Leads** | All Records (CRUD + Convert) | Team Scope (CRUD + Convert) | Assigned Leads (CRUD + Convert) |
| **Opportunities** | All Deals (Full Pipeline) | All Deals (Full Pipeline) | Assigned Opportunities |
| **Follow-Ups** | Enterprise Schedule | Team Schedule | Assigned Follow-Ups |
| **Activities** | Enterprise Activity Feed | Team Activity Feed | Assigned Activities |
| **Reports** | All 5 Reports | All 5 Reports | Not Accessible |
| **User & Role Admin** | Full User Management | Access Denied | Access Denied |
| **Audit Logs** | Full Security Audit Trail | Access Denied | Access Denied |
| **REST APIs** | Authorized Endpoints | Authorized Endpoints | Authorized Endpoints |

---

## 5. Multi-Tier Validation Strategy

AcxiomCRM implements a three-tier validation pipeline:

### 5.1 Client-Side Validation
Implemented via HTML5 attributes, unobtrusive Razor validation scripts, and custom pattern matching:
- **Mandatory Fields:** Customer Name, Email, Phone, Status, Lead Name, Expected Value, Opportunity Name, Amount, Expected Close Date, Follow-up Subject & Date.
- **Email Validation:** Regular expression validating standard RFC email structure.
- **Phone Validation:** Strict Indian mobile number format validation (`^[6-9]\d{9}$`).
- **Real-Time Pipeline Calculation:** Client script dynamically computes Weighted Opportunity Value (`Amount × Probability / 100`) as values change in form controls.

### 5.2 Server-Side Validation
- Validation attributes validated on the server via `ModelState.IsValid`.
- Uniqueness constraints enforced for Customer Code, Email, Phone, and Lead Code.
- Data payloads rejected if client-side validation is bypassed or manipulated.

### 5.3 Business Domain Rules
- **Opportunity Amount:** Must be greater than 0 (`Amount > 0`). Rejects values &le; 0 with error: *"Opportunity Amount must be greater than 0."*
- **Opportunity Probability:** Enforces integer between `0` and `100`. Rejects out-of-range values with error: *"Probability must be between 0 and 100."*
- **Expected Close Date:** Cannot be set in the past for active/open opportunities. Rejects with error: *"Expected Close Date cannot be in the past."*
- **Follow-Up Date:** Planned activities cannot be scheduled earlier than today (`Date >= Today`). Rejects with error: *"Follow-up date cannot be earlier than today."*
- **Lead Status Workflow Transitions:** Enforces valid lifecycle status flow (`New` &rarr; `Contacted` &rarr; `Qualified` &rarr; `Converted` / `Lost`). Invalid transitions (e.g., `New` directly to `Converted`) are rejected.
- **Lead Conversion:** Lead must be in `Qualified` state before conversion to a Customer record is allowed.

---

## 6. Functional Modules Overview

### 6.1 AcxiomCRM Shell & Navigation
- Common responsive layout (`_Layout.cshtml`) with collapsible navigation, user role badge, context menus, and auto-dismissing notifications (`TempData["Success"]` / `TempData["Error"]`).
- Menu items automatically filter based on logged-in user role.

### 6.2 Interactive Dashboard (Chart.js)
- **Top Metric Cards:** Total Customers, Total Leads, Open Leads, Total Opportunities, Won Deals, Lost Deals, Total Active Pipeline Value.
- **Activity Status:** Pending Follow-Ups and Overdue Follow-Ups with quick direct-filter links.
- **Chart.js Visualizations:**
  1. *Lead Status Breakdown:* Doughnut chart showing distribution of leads across lifecycle statuses.
  2. *Opportunities by Stage:* Categorical bar chart for Qualification, Proposal, Negotiation, Won, and Lost.
  3. *Monthly Won Sales Revenue:* Trendline tracking monthly revenue outcomes.

### 6.3 Customer Management
- Create, view details, edit, deactivate (soft delete), and search.
- Multi-field search across Customer Name, Email, Phone, and Company.
- Customer details page displays contact information, company, account owner, and quick action shortcuts.

### 6.4 Lead Management & Conversion
- Lead capture tracking Source, Priority, Expected Value, and Assigned Representative.
- Status transition validation prevents out-of-sequence changes.
- One-click **Lead Conversion** automatically creates a new Customer record, associates the Lead, marks the Lead as `Converted`, and records an audit log entry.

### 6.5 Opportunity Pipeline Management
- Real-time pipeline calculation tracking Amount, Probability, Stage, and Weighted Revenue Forecast.
- Search and filter by Opportunity Name, Customer, Stage, and Status (Open/Closed).

### 6.6 Follow-Up Management
- Scheduling of customer touchpoints (Call, Meeting, Email, Visit).
- Quick filter tabs for **All Tasks**, **Upcoming**, and **Overdue**.
- One-click "Mark Completed" action with audit record generation.

### 6.7 Sales Activity Logging
- Log completed calls, discovery meetings, client emails, and sales tasks with notes and duration.
- Filter by Activity Type, Date, Status, and Representative.

### 6.8 User & Role Administration (Admin Only)
- Admin dashboard displaying all registered accounts, assigned roles, activation state, and lockout status.
- Edit user details, reassign roles, toggle account activation, clear lockouts, and execute administrator password resets.

### 6.9 System Audit Trail (Admin Only)
- Filterable audit viewer tracking all security actions (`LoginSuccess`, `LoginFailed`, `AccountLockedOut`, `Logout`, `Register`, `PasswordReset`, `RoleChange`) and data mutations (`Create`, `Update`, `Delete`, `Convert`).
- Captures User, Action, Entity, RecordId, Old Value, New Value, Timestamp, and IP Address.

### 6.10 Reporting Suite (Admin & Manager)
1. **Customer Report:** Filter by status and account owner with print-friendly formatting.
2. **Lead Report:** Filter by source, qualification status, and owner.
3. **Follow-Up Report:** Filter by status, assigned user, and date range.
4. **Opportunity Report:** Filter by stage, outcome, and deal owner.
5. **Pipeline Report:** Summary of active pipeline broken down by stage with percentage contribution cards.

---

## 7. REST API Documentation

All protected endpoints enforce authentication and role scoping.

| Method | Endpoint | Request Body | Response Codes | Description |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/customers` | None | `200 OK`, `401 Unauthorized` | Returns scoped list of customer DTOs |
| `GET` | `/api/customers/{id}` | None | `200 OK`, `404 Not Found` | Returns single customer DTO |
| `POST` | `/api/customers` | `CreateCustomerDto` | `201 Created`, `400 Bad Request`, `409 Conflict` | Creates customer with validation & uniqueness check |
| `PUT` | `/api/customers/{id}` | `UpdateCustomerDto` | `200 OK`, `400 Bad Request`, `404 Not Found`, `409 Conflict` | Updates customer details |
| `DELETE` | `/api/customers/{id}` | None | `200 OK`, `404 Not Found` | Soft-deactivates customer |
| `GET` | `/api/leads` | None | `200 OK` | Returns scoped lead DTOs |
| `POST` | `/api/leads` | `CreateLeadDto` | `201 Created`, `400 Bad Request` | Creates lead with validation |
| `GET` | `/api/opportunities` | None | `200 OK` | Returns opportunity DTOs with weighted values |
| `POST` | `/api/opportunities` | `CreateOpportunityDto` | `201 Created`, `400 Bad Request` | Creates opportunity with business validation |
| `GET` | `/api/reports/pipeline` | None | `200 OK` | Returns stage-wise pipeline breakdown |

---

## 8. Requirements Verification & Acceptance Matrix

| Item # | Acceptance Criterion | Implementation Status | Evidence / Verification Location |
| :---: | :--- | :---: | :--- |
| **1** | Open CRM without authentication &rarr; Protected pages inaccessible | **PASSED** | Unauthenticated requests redirect to `/Account/Login` |
| **2** | Login with valid credentials &rarr; User reaches Dashboard | **PASSED** | Verified via test users (`admin@acxiomcrm.com`, etc.) |
| **3** | Create Customer with invalid email/phone &rarr; Client validation triggers | **PASSED** | jQuery Validation unobtrusive prevents submission |
| **4** | Tampered HTTP request bypasses browser &rarr; Server rejects invalid data | **PASSED** | Controller `ModelState.IsValid` & custom validations reject request |
| **5** | Create Opportunity with Amount &le; 0 &rarr; Rejected | **PASSED** | `[Range(0.01, ...)]` + Controller business validation reject |
| **6** | Create Opportunity with Probability &gt; 100 &rarr; Rejected | **PASSED** | `[Range(0, 100)]` + Controller business check enforce rule |
| **7** | Expected Close Date in past &rarr; Rejected for active deal | **PASSED** | Validated in Controller & View before save |
| **8** | Follow-Up Date earlier than today &rarr; Rejected for planned task | **PASSED** | Enforced in `FollowUpController.Create` & `Edit` |
| **9** | Login as SalesExecutive &rarr; Scoped to assigned records | **PASSED** | Service queries scope records by `AssignedTo` / `OwnerId` |
| **10** | Login as Manager &rarr; Team pipeline and reports accessible | **PASSED** | `[Authorize(Roles = "Admin,Manager")]` on ReportController |
| **11** | Login as Admin &rarr; User, Role, and Audit administration enabled | **PASSED** | `[Authorize(Roles = "Admin")]` on UserManagement & AuditLog |
| **12** | Create/Update/Delete CRM record &rarr; Audit entry generated | **PASSED** | `IAuditService.LogAsync` records all mutations |
| **13** | Call `/api/customers` &rarr; Authorized JSON DTO response | **PASSED** | `CustomersApiController` returns DTO list |
| **14** | Open Dashboard &rarr; 8 KPI cards & Chart.js charts display data | **PASSED** | `Dashboard/Index.cshtml` renders Chart.js with seeded metrics |

---

## 9. Verification & Execution Instructions

### Pre-Configured Test Accounts

| Role | Username / Email | Password |
| :--- | :--- | :--- |
| **Admin** | `admin@acxiomcrm.com` | `Admin@123` |
| **Manager** | `manager@acxiomcrm.com` | `Manager@123` |
| **Sales Executive** | `sales@acxiomcrm.com` | `Sales@123` |

### Running the Application

```bash
cd c:\Users\subha\OneDrive\Desktop\crm\AcxiomCRM
dotnet run
```
Open **`http://localhost:5001`** in any browser.

The database file `AcxiomCRM.db` will be initialized and pre-seeded automatically on first startup.

