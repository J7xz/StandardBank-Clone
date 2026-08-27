# 🏦 Standard Bank - Online Banking Platform

A full-featured online banking application built with ASP.NET MVC, Entity Framework, and SQL Server. This platform provides a complete banking experience including account management, fund transfers, bill payments, savings goals, budgeting with AI insights, and an admin dashboard.

## ✨ Features

### 🔐 Authentication & Profile
- User registration with email, phone, and ID number
- Secure login/logout with password reset
- Profile management with edit functionality
- Password change with validation

### 📊 Dashboard
- Real-time balance overview across all accounts
- Quick action buttons (Send Money, Pay Bills, New Account)
- Recent transactions with filtering
- Savings goals progress tracking
- Budget spending summary with progress bars

### 💰 Core Banking
- **Multiple Account Types**: Cheque, Savings, and Student accounts
- **Internal Transfers**: Transfer between your own accounts
- **External Transfers**: Send money to other banks with beneficiary management
- **Transaction History**: Filter by date, type, account, and search
- **PDF Statements**: Export account statements

### 📝 Bill Payments
- Pay bills for Electricity, Water, DSTV, Internet, Municipality, and Education
- Payment history with filters
- Printable receipts

### 🎯 Savings Goals
- Create and track savings goals with target amounts and deadlines
- Add/withdraw funds with progress visualization
- Automatic completion tracking

### 📈 Budget Tracker
- Category-based monthly budgets (Groceries, Transport, Entertainment, etc.)
- Real-time spending tracking
- AI-powered spending insights and alerts
- Yearly summary view

### 👥 Beneficiaries
- Save frequent recipients for quick transfers
- Edit and delete beneficiary details

### 🔔 Notifications
- In-app notification system
- Mark as read/unread
- Admin broadcast feature

### 🔧 Admin Panel
- User management (view, lock, unlock)
- Transaction monitoring with filters
- System logs with activity tracking
- Generate reports

## 🛠️ Technology Stack

| Layer | Technology |
|-------|------------|
| **Backend** | ASP.NET MVC (.NET Framework) |
| **ORM** | Entity Framework 6 |
| **Database** | SQL Server / LocalDB |
| **Frontend** | HTML5, CSS3, JavaScript, jQuery |
| **UI Framework** | Bootstrap 5 |
| **Authentication** | ASP.NET Identity |
| **Icons** | Font Awesome 6 |
| **Fonts** | Google Fonts (Inter) |

## 🚀 Getting Started

### Prerequisites
- Visual Studio 2022 or later
- .NET Framework 4.8
- SQL Server or LocalDB

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/yourusername/StandardBank.git
cd StandardBank
Update the database connection string in Web.config:

xml
<connectionStrings>
  <add name="DefaultConnection" 
       connectionString="Data Source=(LocalDb)\MSSQLLocalDB;Database=StandardBankDB;Trusted_Connection=True;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
Run database migrations in Package Manager Console:

powershell
Update-Database
Create admin user (optional):

Run the project and navigate to /Setup/CreateAdmin

Or use the Seed method in Migrations/Configuration.cs

Run the application (F5 in Visual Studio)

Default Login
Admin: admin@standardbank.com / Admin@123

📁 Project Structure
text
StandardBank/
├── Controllers/          # MVC Controllers
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
├── Models/               # Entity Models
│   ├── User.cs
│   ├── Account.cs
│   ├── Transaction.cs
│   ├── Beneficiary.cs
│   ├── BillPayment.cs
│   ├── Budget.cs
│   ├── SavingsGoal.cs
│   ├── Notification.cs
│   └── AdminLog.cs
├── ViewModels/           # View Models
├── Views/                # Razor Views
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
├── Content/              # CSS, Images, Assets
├── Scripts/              # JavaScript files
├── App_Start/            # Startup configuration
├── Migrations/           # EF Migrations
└── Web.config            # Application configuration
🎨 UI Features
Dark Navy & Gold Theme - Standard Bank inspired branding

Responsive Design - Works on desktop, tablet, and mobile

Sidebar Navigation - Clean and organized menu

Animated Elements - Smooth transitions and loading states

Interactive Charts - Visual spending insights

🔒 Security Features
ASP.NET Identity authentication

Role-based authorization (Admin/User)

Password hashing and validation

Anti-forgery tokens on all forms

SQL injection protection via Entity Framework

XSS protection with Razor encoding

📸 Screenshots
Dashboard	Accounts
Dashboard view with balance, quick actions, and recent transactions	Account list with balance and details
Transfers	Budget Insights
Internal and external transfer functionality	AI-powered spending analysis and alerts
Savings Goals	Admin Panel
Track savings progress with visual goals	User management and transaction monitoring
🚀 Future Enhancements
□ Two-Factor Authentication (2FA)
□ Email notifications
□ Mobile app API
□ Investment tracking
□ Tax reports
□ Multi-language support
🤝 Contributing
Fork the repository

Create your feature branch (git checkout -b feature/AmazingFeature)

Commit your changes (git commit -m 'Add some AmazingFeature')

Push to the branch (git push origin feature/AmazingFeature)

Open a Pull Request

📄 License
This project is licensed under the MIT License - see the LICENSE file for details.

🙏 Acknowledgments
Bootstrap for the UI framework

Font Awesome for icons

Google Fonts for typography

ASP.NET Identity for authentication



Made with ❤️ and C#

text

---

## Save this file:

1. Create a file called `README.md` in your project root
2. Copy and paste the content above
3. Replace `yourusername` with your actual GitHub username
4. Add, commit, and push:

```cmd
git add README.md
git commit -m "Add README"
git push
