# Mini Accounting System - ASP.NET Core MVC

The Mini Accounting System is a web-based application built with ASP.NET Core MVC that provides essential functionalities for managing a small business's accounting records. It focuses on core accounting tasks like managing a Chart of Accounts and performing Voucher Entries (Journal, Payment, Receipt, Contra). The system aims to provide a clear, user-friendly interface for basic financial record-keeping.

## Features

* **User Authentication & Authorization:**
    * Secure admin login system.
    * Admin (Username: `Admin`, Password: `Admin@123`) to control access to different modules.
* **Chart of Accounts Management:**
    * View, Add, Edit, and Delete accounting accounts.
* **Voucher Entry Module:**
    * Create various types of vouchers (Journal, Payment, Receipt, Contra).
    * Dynamic addition/removal of debit and credit line items within a voucher.
    * Real-time validation to ensure total debits equal total credits.
    * View, Edit, and Delete existing vouchers.
    * Efficient data storage using SQL Server Table-Valued Parameters (TVPs).
* **Database Interaction:**
    * Utilizes ADO.NET for direct and efficient interaction with SQL Server.
    * Leverages Stored Procedures for most CRUD operations, enhancing security and performance.

## Technologies Used

* **Backend:**
    * ASP.NET Core 8.0 MVC
    * C#
    * Microsoft SQL Server
    * ADO.NET
* **Frontend:**
    * HTML5
    * CSS3 (Bootstrap 5 for styling)
    * JavaScript (jQuery for dynamic UI)
* **Development Tools:**
    * Visual Studio 2022
    * SQL Server Management Studio (SSMS)
    * Git / GitHub

### Database Setup

1.  **Please restore the "MiniAccount_DB" database in MS-SQL Server.
2.  **SQL Scripts:**
    **The scripts in the following order against your newly created database:
        * `01_Tables.sql` (Creates `Accounts`, `VoucherMaster`, `VoucherDetails` tables)
        * `02_Table_Valued_Parameters.sql` (Creates `VoucherDetailType`)
        * `03_Stored_Procedures.sql` (Creates `sp_ManageChartOfAccounts`, `sp_SaveVoucher`, `sp_GetVoucherById`, `sp_UpdateVoucher`, `sp_DeleteVoucher`)
        * `04_Seed_Data.sql` (Populates admin user).
3.  **Update Connection String:**
    * Open your project in Visual Studio.
    * Open `appsettings.json`.
    * Update the `DefaultConnection` string to point to your SQL Server instance:
        "ConnectionStrings": {
            "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=MiniAccountDB;Integrated Security=True;TrustServerCertificate=True"
                    }
    *If you're using SQL Server authentication, replace `Integrated Security=True` with `User ID=YOUR_USERNAME;Password=YOUR_PASSWORD`.*

### Running the Application

1.  Open the `MiniAccountingSystem.sln` file in Visual Studio.
2.  Build the solution.
3.  Run the application.
4.  The application should open in your default web browser.

## Usage

1.  **Register/Login:**
    * You can log in with the default admin user (e.g., Username: `Admin`, Password: `Admin@123`).

2.  **Navigate:** Use the navigation bar to access:
    * **Chart of Accounts:** Manage accounting ledger.
    * **View Vouchers:** See a list of all recorded vouchers.