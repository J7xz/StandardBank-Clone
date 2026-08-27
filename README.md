Standard Bank clone - Online Banking Platform
A full-featured online banking application built with ASP.NET MVC (.NET Framework), Entity Framework, and SQL Server.

Features
Authentication
User registration with Full Name, Email, Phone Number, and ID Number

Secure login with email and password

Password reset functionality

Profile management (view and edit personal information)

Change password with current password verification

Role-based access (Customer and Admin)

Dashboard
Total balance overview across all accounts

Quick action buttons: Send Money, Pay Bills, New Account

Account summary with balances and status

Recent transactions (last 5)

Savings goals progress tracking

Monthly budget tracking with progress bars

Balance toggle for privacy (hide/show)

Banking
Multiple account types: Cheque, Savings, Student

Create new accounts with optional initial deposit

Internal transfers between your own accounts

External transfers to other banks and beneficiaries

Beneficiary management (save, edit, delete)

Transaction history with filters (account, type, date range, search)

Account details with recent transactions

Bill Payments
Pay bills for Electricity, Water, DSTV, Internet, Municipality, Education

Payment history with filters

Printable receipts

Reference number tracking

Savings Goals
Create savings targets with deadlines

Add funds from accounts to savings goals

Withdraw funds from savings goals to accounts

Visual progress tracking

Deadline monitoring with color-coded alerts

Budget Tracking
Category-based monthly budgets (Groceries, Transport, Entertainment, etc.)

Real-time spending tracking against budgets

AI-powered spending insights (highest spending category, month-over-month trends, weekend patterns)

Budget alerts when nearing or exceeding limits

Yearly summary view

Notifications
In-app notification system

Read/Unread status tracking

Mark individual or all notifications as read

Delete individual or all read notifications

Admin broadcast to all users

Admin Panel
System overview dashboard with metrics (users, accounts, transactions)

User management (view, search, filter, lock, unlock)

User details view with accounts and transactions

Transaction monitoring with filtering

Admin activity logs with timestamps and IP addresses

Report generation with date range filtering

Technology Stack
Layer	Technology
Backend	ASP.NET MVC (.NET Framework 4.8)
ORM	Entity Framework 6 (Code First)
Database	SQL Server / LocalDB
Authentication	ASP.NET Identity
Frontend	HTML5, CSS3, JavaScript, jQuery
UI Framework	Bootstrap 5
Icons	Font Awesome 6
Fonts	Google Fonts (Inter)
Database Schema
Model	Purpose
User	Application user with profile information
Account	Bank accounts (Cheque, Savings, Student)
Transaction	All financial transactions (debits and credits)
Beneficiary	Saved recipients for transfers
BillPayment	Bill payment records
SavingsGoal	User savings targets
Budget	Monthly spending limits by category
Notification	In-app notifications
AdminLog	Admin activity tracking
Clone the Repository
bash
git clone https://github.com/J7xz/StandardBank-Clone.git
cd StandardBank-Clone
Installation
Prerequisites
Visual Studio 2022 or later

.NET Framework 4.8

SQL Server or LocalDB

Steps
Clone the repository (see above)

Open the solution in Visual Studio

Update the connection string in Web.config:

xml
<connectionStrings>
  <add name="DefaultConnection" 
       connectionString="Data Source=(LocalDb)\MSSQLLocalDB;Database=StandardBankDB;Trusted_Connection=True;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
Run database migrations in Package Manager Console:

powershell
Enable-Migrations
Add-Migration InitialCreate
Update-Database
Create an admin user:

Run the application

Navigate to /Setup/CreateAdmin

Click the "Create Admin" button

Run the application (F5)

Default Credentials
Role	Email	Password
Admin	admin@standardbank.com	Admin@123
Project Structure
text
StandardBank/
├── Controllers/
│   ├── AccountController.cs
│   ├── AdminController.cs
│   ├── BankingController.cs
│   ├── BeneficiaryController.cs
│   ├── BillPaymentController.cs
│   ├── BudgetController.cs
│   ├── DashboardController.cs
│   ├── HomeController.cs
│   ├── NotificationController.cs
│   ├── ProfileController.cs
│   └── SavingsController.cs
├── Models/
│   ├── User.cs
│   ├── Account.cs
│   ├── Transaction.cs
│   ├── Beneficiary.cs
│   ├── BillPayment.cs
│   ├── Budget.cs
│   ├── SavingsGoal.cs
│   ├── Notification.cs
│   └── AdminLog.cs
├── ViewModels/
├── Views/
│   ├── Account/
│   ├── Admin/
│   ├── Banking/
│   ├── Beneficiary/
│   ├── BillPayment/
│   ├── Budget/
│   ├── Dashboard/
│   ├── Home/
│   ├── Notification/
│   ├── Profile/
│   └── Savings/
├── App_Start/
│   └── IdentityConfig.cs
├── Migrations/
├── Content/
├── Scripts/
└── Web.config
Security
ASP.NET Identity for authentication and authorization

Role-based access control (Admin / User)

Anti-forgery tokens on all forms

SQL injection protection via Entity Framework

XSS protection with Razor encoding

Password hashing and validation

License
MIT License
