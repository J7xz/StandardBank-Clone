namespace StandardBank.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Accounts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccountNumber = c.String(nullable: false, maxLength: 20),
                        AccountType = c.String(nullable: false),
                        Balance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DateOpened = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        AccountName = c.String(),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Transactions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TransactionType = c.String(nullable: false),
                        TransactionCategory = c.String(nullable: false),
                        Description = c.String(),
                        TransactionDate = c.DateTime(nullable: false),
                        Status = c.String(),
                        BalanceAfterTransaction = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ReferenceNumber = c.String(),
                        AccountId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Accounts", t => t.AccountId, cascadeDelete: true)
                .Index(t => t.AccountId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FullName = c.String(),
                        PhoneNumber = c.String(),
                        IDNumber = c.String(),
                        DateRegistered = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        LastLoginDate = c.DateTime(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.Beneficiaries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        AccountNumber = c.String(nullable: false, maxLength: 20),
                        BankName = c.String(nullable: false),
                        Nickname = c.String(),
                        DateAdded = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.BillPayments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BillType = c.String(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ReferenceNumber = c.String(nullable: false),
                        PaymentDate = c.DateTime(nullable: false),
                        Status = c.String(),
                        Description = c.String(),
                        UserId = c.String(nullable: false, maxLength: 128),
                        AccountId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Accounts", t => t.AccountId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.AccountId);
            
            CreateTable(
                "dbo.Budgets",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Category = c.String(nullable: false),
                        AllocatedAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SpentAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Month = c.Int(nullable: false),
                        Year = c.Int(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false),
                        Message = c.String(nullable: false),
                        NotificationType = c.String(),
                        DateSent = c.DateTime(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                        DateRead = c.DateTime(),
                        LinkUrl = c.String(),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.SavingsGoals",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GoalName = c.String(nullable: false),
                        TargetAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CurrentAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Deadline = c.DateTime(),
                        DateCreated = c.DateTime(nullable: false),
                        IsCompleted = c.Boolean(nullable: false),
                        Description = c.String(),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AdminLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Action = c.String(nullable: false),
                        UserId = c.String(),
                        TargetUserId = c.String(),
                        Details = c.String(),
                        Timestamp = c.DateTime(nullable: false),
                        IPAddress = c.String(),
                        UserAgent = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.SavingsGoals", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Notifications", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Budgets", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BillPayments", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BillPayments", "AccountId", "dbo.Accounts");
            DropForeignKey("dbo.Beneficiaries", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Accounts", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Transactions", "AccountId", "dbo.Accounts");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.SavingsGoals", new[] { "UserId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.Notifications", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.Budgets", new[] { "UserId" });
            DropIndex("dbo.BillPayments", new[] { "AccountId" });
            DropIndex("dbo.BillPayments", new[] { "UserId" });
            DropIndex("dbo.Beneficiaries", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Transactions", new[] { "AccountId" });
            DropIndex("dbo.Accounts", new[] { "UserId" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.AdminLogs");
            DropTable("dbo.SavingsGoals");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.Notifications");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.Budgets");
            DropTable("dbo.BillPayments");
            DropTable("dbo.Beneficiaries");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Transactions");
            DropTable("dbo.Accounts");
        }
    }
}
