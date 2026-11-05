# AcxiomCRM — Enterprise Customer Relationship Management System

**AcxiomCRM** is an enterprise-grade ASP.NET Core MVC application developed with a layered architecture, ASP.NET Core Identity authentication, role-based authorization (Admin, Manager, SalesExecutive), client and server-side validation, business rule enforcement, append-oriented security audit logging, Chart.js analytics dashboard, and secured RESTful APIs.

---

## 1. Quick Start & Execution

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher
- Any modern web browser (Edge, Chrome, Firefox)

### Running the Application
From the solution root directory:
```bash
cd AcxiomCRM
dotnet run
```
Navigate to:
- **Web UI:** `https://localhost:7001` or `http://localhost:5001`
- Automatically redirects unauthenticated requests to `/Account/Login`.

---

## 2. Pre-Configured Test Credentials

The database automatically initializes and seeds roles, users, and realistic demo records on first startup:

| Role | Email / Username | Password | Access Scope |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@acxiomcrm.com` | `Admin@123` | Full access: User & Role Administration, Audit Logs, CRM Records, All Reports, System Settings |
| **Manager** | `manager@acxiomcrm.com` | `Manager@123` | Team oversight: View & manage team pipeline, opportunities, leads, follow-ups, and executive reports |
| **SalesExecutive** | `sales@acxiomcrm.com` | `Sales@123` | Individual scope: Manage assigned customers, leads, opportunities, follow-ups, and daily activities |

---

## 3. Architecture & Technical Stack

```
                          ┌────────────────────────┐
                          │   Presentation Layer   │
                          │ (Bootstrap 5, Chart.js)│
                          └───────────┬────────────┘
                                      │
                          ┌───────────▼────────────┐
                          │   Application Layer    │
                          │ (MVC & REST Controllers│
                          │ ViewModels & DTOs)     │
                          └───────────┬────────────┘
                                      │
                          ┌───────────▼────────────┐
                          │  Domain/Business Layer │
                          │  (Validation, Services │
                          │   Workflow Transition) │
                          └───────────┬────────────┘
                                      │
                          ┌───────────▼────────────┐
                          │   Data Access Layer    │
                          │(EF Core 8, Repositories│
                          │  ApplicationDbContext) │
                          └───────────┬────────────┘
                                      │
                          ┌───────────▼────────────┐
                          │     Database Layer     │
                          │    (SQLite / RDBMS)    │
                          └────────────────────────┘
          ──────────────────────────────────────────────────
          Cross-Cutting: Identity Security + Audit Logging + Anti-Forgery
```

- **Framework:** ASP.NET Core 8.0 MVC + Web API
- **Identity & Security:** ASP.NET Core Identity with PBKDF2 adaptive password hashing, 15-minute account lockout after 5 failed attempts, Anti-Forgery Tokens (`[ValidateAntiForgeryToken]`)
- **ORM & Database:** Entity Framework Core 8.0 with SQLite (`AcxiomCRM.db`), parameterized queries preventing SQL injection
- **Frontend:** Bootstrap 5.3, Bootstrap Icons, jQuery 3.7, jQuery Validation & Unobtrusive Validation, Chart.js 4.4

---

## 4. Key Functional Modules

### 4.1 Authentication & Security
- Secure sign-in and sign-out with sliding session expiration.
- Identity password policy: minimum 8 characters, requiring uppercase, lowercase, digit, and non-alphanumeric character.
- Account lockout policy: 5 consecutive failed login attempts locks the account for 15 minutes.
- Administrators can view lockout status and clear lockouts via one-click unlock.

### 4.2 Role-Based Dashboard
- Real-time KPI Cards: Total Customers, Total Leads, Open Leads, Total Opportunities, Open Deals, Won Deals, Lost Deals, Total Pipeline Value, Pending Follow-ups, and Overdue Follow-ups.
- Interactive Chart.js visualizations:
  1. **Lead Status Breakdown:** Doughnut chart (New, Contacted, Qualified, Converted, Lost).
  2. **Opportunities by Stage:** Bar chart (Qualification, Proposal, Negotiation, Won, Lost).
  3. **Monthly Won Sales Revenue:** Line chart tracking cumulative closed revenue over time.

### 4.3 Customer Management
- Complete CRUD operations with unique Customer Code (`CUST-xxxxx`).
- Dual-tier validation: Indian 10-digit mobile number format regex (`^[6-9]\d{9}$`), valid email, and duplicate detection preventing conflicting emails/phones.
- Customer details page linking related leads, opportunities, follow-ups, and activities.

### 4.4 Lead Management & Qualification Workflow
- Status workflow enforcement: `New` &rarr; `Contacted` &rarr; `Qualified` &rarr; `Converted` / `Lost`.
- One-click **Lead Conversion**: Qualified leads convert directly into active Customer records with preserved owner assignment and automatic audit entry.

### 4.5 Opportunity Pipeline Management
- Real-time **Weighted Pipeline Calculation**: `Amount × (Probability / 100)`.
- Enforces strict business validations:
  - Opportunity Amount must be greater than 0 (`Amount > 0`).
  - Win Probability must be between 0% and 100%.
  - Expected Close Date cannot be set in the past for active deals.

### 4.6 Follow-Up Scheduling & Task Management
- Schedule calls, meetings, emails, and site visits tied to customers, leads, or opportunities.
- Dedicated filters for **All**, **Upcoming**, and **Overdue** follow-up tasks.
- Prevents scheduling new/planned follow-up dates earlier than today.

### 4.7 Sales Activity Logging
- Log completed calls, client meetings, emails, and internal tasks.
- Filter by activity type, date, status, and representative.

### 4.8 User & Role Administration (Admin Only)
- Manage user profiles, assign single primary role (Admin, Manager, SalesExecutive).
- Activate or deactivate employee accounts.
- Administrative password reset with full validation.
- Clear account lockouts for locked users.

### 4.9 Audit Log Trail (Admin Only)
- Append-oriented audit logging capturing:
  - Timestamp (UTC)
  - User Identifier & Name
  - Action (LoginSuccess, LoginFailed, AccountLockedOut, Logout, Create, Update, Delete, Convert, RoleChange, PasswordReset)
  - Module/Entity Name
  - Record ID
  - Old Value vs. New Value
  - Client IP Address

### 4.10 Reports (Admin & Manager)
- **Customer Report:** Filter by status and account owner with print/export.
- **Lead Report:** Filter by acquisition source, status, and sales executive.
- **Follow-Up Report:** Filter by date range, status, and representative.
- **Opportunity Report:** Filter by deal stage, open/closed status, and owner.
- **Pipeline Report:** Stage-wise financial breakdown with percentage of total pipeline.

---

## 5. REST API Specification

All REST endpoints return standard HTTP status codes (`200 OK`, `201 Created`, `400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `409 Conflict`) and use DTOs to avoid exposing internal entity structures or sensitive credentials.

| Method | Endpoint | Description | Auth Scope |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/customers` | Retrieve list of customers in user's authorized scope | Authenticated |
| `GET` | `/api/customers/{id}` | Retrieve customer details by ID | Authenticated |
| `POST` | `/api/customers` | Create a new customer record with validation | Authenticated |
| `PUT` | `/api/customers/{id}` | Update an existing customer record | Authenticated |
| `DELETE` | `/api/customers/{id}` | Deactivate customer record | Authenticated |
| `GET` | `/api/leads` | Retrieve leads in user's scope | Authenticated |
| `POST` | `/api/leads` | Create a new lead record | Authenticated |
| `GET` | `/api/opportunities` | Retrieve opportunity deals | Authenticated |
| `POST` | `/api/opportunities` | Create an opportunity with business validation | Authenticated |
| `GET` | `/api/reports/pipeline` | Retrieve stage-wise pipeline metrics | Manager / Admin |

---

## 6. Acceptance Testing Scenarios

1. **Anonymous Access Block:** Open `https://localhost:7001/Customer` without logging in &rarr; Redirects to `/Account/Login`.
2. **Account Lockout:** Enter wrong password 5 consecutive times for an account &rarr; Account locks for 15 minutes and generates an `AccountLockedOut` audit event.
3. **Client-Side Validation:** On Customer Create, enter an invalid phone number or leave Name empty &rarr; Red validation errors appear immediately before submission.
4. **Server-Side Validation Bypass:** Submit a crafted POST request with invalid data &rarr; Server rejects request and returns error message without database corruption.
5. **Opportunity Business Rules:**
   - Set Amount = 0 &rarr; Rejected with *"Opportunity Amount must be greater than 0."*
   - Set Probability = 150 &rarr; Rejected with *"Probability must be between 0 and 100."*
   - Set Expected Close Date = Yesterday &rarr; Rejected with *"Expected Close Date cannot be in the past."*
6. **Follow-Up Business Rules:**
   - Set Follow-Up Date = Yesterday &rarr; Rejected with *"Follow-up date cannot be earlier than today."*
7. **Lead Conversion:** Change lead status to `Qualified`, click `Convert to Customer` &rarr; Customer account is created and Lead is updated to `Converted`.
8. **Role-Based Scoping:**
   - Log in as `sales@acxiomcrm.com` &rarr; Only see assigned CRM records; Administration and Audit menus are hidden.
   - Log in as `admin@acxiomcrm.com` &rarr; Full system access, audit trail, and user management enabled.

