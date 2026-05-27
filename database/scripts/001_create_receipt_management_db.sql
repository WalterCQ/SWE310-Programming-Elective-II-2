IF DB_ID(N'ReceiptManagementDb') IS NULL
BEGIN
    CREATE DATABASE ReceiptManagementDb;
END
GO

USE ReceiptManagementDb;
GO

IF OBJECT_ID(N'dbo.ReceiptManagementReceiptItems', N'U') IS NOT NULL DROP TABLE dbo.ReceiptManagementReceiptItems;
IF OBJECT_ID(N'dbo.ReceiptManagementReceipts', N'U') IS NOT NULL DROP TABLE dbo.ReceiptManagementReceipts;
IF OBJECT_ID(N'dbo.ReceiptManagementExpenseCategories', N'U') IS NOT NULL DROP TABLE dbo.ReceiptManagementExpenseCategories;
IF OBJECT_ID(N'dbo.ReceiptManagementVendors', N'U') IS NOT NULL DROP TABLE dbo.ReceiptManagementVendors;
GO

CREATE TABLE dbo.ReceiptManagementVendors
(
    VendorId INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(120) NOT NULL,
    ContactPerson NVARCHAR(100) NULL,
    Phone NVARCHAR(30) NULL,
    Email NVARCHAR(120) NULL,
    Address NVARCHAR(250) NULL,
    TaxRegistrationNumber NVARCHAR(60) NULL,
    Notes NVARCHAR(300) NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ReceiptManagementVendors_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ReceiptManagementVendors PRIMARY KEY (VendorId),
    CONSTRAINT UQ_ReceiptManagementVendors_Name UNIQUE (Name)
);
GO

CREATE TABLE dbo.ReceiptManagementExpenseCategories
(
    ExpenseCategoryId INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(80) NOT NULL,
    Description NVARCHAR(250) NULL,
    MonthlyBudget DECIMAL(12,2) NOT NULL,
    ColorHex NVARCHAR(7) NOT NULL,
    IconName NVARCHAR(40) NOT NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ReceiptManagementExpenseCategories_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ReceiptManagementExpenseCategories PRIMARY KEY (ExpenseCategoryId),
    CONSTRAINT UQ_ReceiptManagementExpenseCategories_Name UNIQUE (Name),
    CONSTRAINT CK_ReceiptManagementExpenseCategories_MonthlyBudget CHECK (MonthlyBudget >= 0),
    CONSTRAINT CK_ReceiptManagementExpenseCategories_ColorHex CHECK (ColorHex LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]')
);
GO

CREATE TABLE dbo.ReceiptManagementReceipts
(
    ReceiptId INT IDENTITY(1,1) NOT NULL,
    ReceiptNumber NVARCHAR(40) NOT NULL,
    ReceiptDate DATETIME2 NOT NULL,
    VendorId INT NULL,
    VendorNameSnapshot NVARCHAR(120) NOT NULL,
    ExpenseCategoryId INT NULL,
    CategoryNameSnapshot NVARCHAR(80) NOT NULL,
    SubtotalAmount DECIMAL(12,2) NOT NULL,
    TaxAmount DECIMAL(12,2) NOT NULL,
    TotalAmount DECIMAL(12,2) NOT NULL,
    PaymentMethod NVARCHAR(30) NOT NULL,
    Status NVARCHAR(30) NOT NULL,
    Notes NVARCHAR(500) NULL,
    ImageUrl NVARCHAR(300) NULL,
    CurrencyCode NVARCHAR(3) NOT NULL CONSTRAINT DF_ReceiptManagementReceipts_CurrencyCode DEFAULT 'MYR',
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ReceiptManagementReceipts_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT PK_ReceiptManagementReceipts PRIMARY KEY (ReceiptId),
    CONSTRAINT UQ_ReceiptManagementReceipts_ReceiptNumber UNIQUE (ReceiptNumber),
    CONSTRAINT FK_ReceiptManagementReceipts_Vendors FOREIGN KEY (VendorId)
        REFERENCES dbo.ReceiptManagementVendors (VendorId) ON DELETE SET NULL,
    CONSTRAINT FK_ReceiptManagementReceipts_ExpenseCategories FOREIGN KEY (ExpenseCategoryId)
        REFERENCES dbo.ReceiptManagementExpenseCategories (ExpenseCategoryId) ON DELETE SET NULL,
    CONSTRAINT CK_ReceiptManagementReceipts_SubtotalAmount CHECK (SubtotalAmount >= 0),
    CONSTRAINT CK_ReceiptManagementReceipts_TaxAmount CHECK (TaxAmount >= 0),
    CONSTRAINT CK_ReceiptManagementReceipts_TotalAmount CHECK (TotalAmount >= 0),
    CONSTRAINT CK_ReceiptManagementReceipts_CurrencyCode CHECK (CurrencyCode = 'MYR'),
    CONSTRAINT CK_ReceiptManagementReceipts_PaymentMethod CHECK (PaymentMethod IN ('Cash', 'CreditCard', 'DebitCard', 'EWallet', 'BankTransfer')),
    CONSTRAINT CK_ReceiptManagementReceipts_Status CHECK (Status IN ('Draft', 'Recorded', 'Reimbursed', 'Archived'))
);
GO

CREATE TABLE dbo.ReceiptManagementReceiptItems
(
    ReceiptItemId INT IDENTITY(1,1) NOT NULL,
    ReceiptId INT NOT NULL,
    Description NVARCHAR(160) NOT NULL,
    Quantity DECIMAL(10,2) NOT NULL,
    UnitPrice DECIMAL(12,2) NOT NULL,
    LineTotal DECIMAL(12,2) NOT NULL,
    Notes NVARCHAR(250) NULL,
    CONSTRAINT PK_ReceiptManagementReceiptItems PRIMARY KEY (ReceiptItemId),
    CONSTRAINT FK_ReceiptManagementReceiptItems_Receipts FOREIGN KEY (ReceiptId)
        REFERENCES dbo.ReceiptManagementReceipts (ReceiptId) ON DELETE CASCADE,
    CONSTRAINT CK_ReceiptManagementReceiptItems_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_ReceiptManagementReceiptItems_UnitPrice CHECK (UnitPrice >= 0),
    CONSTRAINT CK_ReceiptManagementReceiptItems_LineTotal CHECK (LineTotal >= 0)
);
GO

INSERT INTO dbo.ReceiptManagementVendors
    (Name, ContactPerson, Phone, Email, Address, TaxRegistrationNumber, Notes)
VALUES
    ('Lotus Malaysia', 'Customer Care', '+603-7726-8888', 'care@lotus.com.my', 'Kuala Lumpur, Malaysia', 'MY-LOTUS-001', 'Daily groceries and dining items'),
    ('Touch n Go', 'Support Desk', '+603-2714-8888', 'support@touchngo.com.my', 'Bangsar South, Kuala Lumpur', 'MY-TNG-002', 'Transport and e-wallet expenses'),
    ('Popular Bookstore', 'Store Manager', '+603-9179-8888', 'hello@popular.com.my', 'Mid Valley Megamall, Kuala Lumpur', 'MY-POP-003', 'Stationery and books'),
    ('Grab Malaysia', 'Grab Support', '+603-2788-1300', 'support.my@grab.com', 'Petaling Jaya, Selangor', 'MY-GRB-004', 'Ride and delivery receipts'),
    ('Maxis', 'Billing Support', '+603-7492-2123', 'care@maxis.com.my', 'Kuala Lumpur, Malaysia', 'MY-MXS-005', 'Mobile and internet bills'),
    ('Shopee Malaysia', 'Marketplace Support', '+603-2777-9222', 'support@shopee.com.my', 'Kuala Lumpur, Malaysia', 'MY-SHP-006', 'Online purchases'),
    ('AirAsia', 'Travel Desk', '+603-8660-4333', 'support@airasia.com', 'Sepang, Selangor', 'MY-AIR-007', 'Travel bookings'),
    ('Guardian Pharmacy', 'Store Supervisor', '+603-5569-4888', 'support@guardian.com.my', 'Subang Jaya, Selangor', 'MY-GDN-008', 'Healthcare products'),
    ('Golden Screen Cinemas', 'Guest Service', '+603-7713-7888', 'support@gsc.com.my', 'Kuala Lumpur, Malaysia', 'MY-GSC-009', 'Entertainment receipts'),
    ('Udemy', 'Learner Support', NULL, 'support@udemy.com', 'Online Platform', 'MY-UDM-010', 'Online education courses');
GO

INSERT INTO dbo.ReceiptManagementExpenseCategories
    (Name, Description, MonthlyBudget, ColorHex, IconName)
VALUES
    ('Food & Dining', 'Meals, drinks, cafes, and restaurants', 800.00, '#FF4D6D', 'utensils'),
    ('Transportation', 'Public transport, e-wallet transit, rides, and fuel', 450.00, '#00F5FF', 'car'),
    ('Office Supplies', 'Stationery, paper, books, and desk tools', 300.00, '#FFD166', 'briefcase'),
    ('Groceries', 'Household grocery purchases', 900.00, '#7CFF6B', 'shopping-bag'),
    ('Utilities', 'Phone, internet, power, and water bills', 550.00, '#A855F7', 'zap'),
    ('Travel', 'Flights, hotels, and trip expenses', 1200.00, '#FB7185', 'plane'),
    ('Software', 'Apps, tools, storage, and subscriptions', 250.00, '#38BDF8', 'monitor'),
    ('Healthcare', 'Pharmacy, clinic, and wellness purchases', 400.00, '#34D399', 'heart-pulse'),
    ('Entertainment', 'Movies, games, events, and leisure', 350.00, '#F97316', 'ticket'),
    ('Education', 'Courses, learning materials, and training', 500.00, '#FACC15', 'graduation-cap');
GO

INSERT INTO dbo.ReceiptManagementReceipts
    (ReceiptNumber, ReceiptDate, VendorId, VendorNameSnapshot, ExpenseCategoryId, CategoryNameSnapshot, SubtotalAmount, TaxAmount, TotalAmount, PaymentMethod, Status, Notes, ImageUrl)
VALUES
    ('RCP-2026-0001', '2026-01-10', 1, 'Lotus Malaysia', 1, 'Food & Dining', 23.40, 1.40, 24.80, 'EWallet', 'Recorded', 'Lunch and drink after class', NULL),
    ('RCP-2026-0002', '2026-01-22', 4, 'Grab Malaysia', 2, 'Transportation', 35.00, 0.00, 35.00, 'EWallet', 'Recorded', 'Trip from campus to home', NULL),
    ('RCP-2026-0003', '2026-02-04', 3, 'Popular Bookstore', 3, 'Office Supplies', 37.00, 2.22, 39.22, 'DebitCard', 'Recorded', 'Assignment materials', NULL),
    ('RCP-2026-0004', '2026-02-18', 1, 'Lotus Malaysia', 4, 'Groceries', 48.60, 0.00, 48.60, 'Cash', 'Recorded', 'Weekly groceries', NULL),
    ('RCP-2026-0005', '2026-03-08', 5, 'Maxis', 5, 'Utilities', 79.00, 4.74, 83.74, 'BankTransfer', 'Recorded', 'Monthly mobile plan', NULL),
    ('RCP-2026-0006', '2026-03-21', 6, 'Shopee Malaysia', 7, 'Software', 19.90, 1.19, 21.09, 'CreditCard', 'Recorded', 'Cloud storage subscription', NULL),
    ('RCP-2026-0007', '2026-04-02', 7, 'AirAsia', 6, 'Travel', 208.00, 12.48, 220.48, 'CreditCard', 'Draft', 'Upcoming semester break trip', NULL),
    ('RCP-2026-0008', '2026-04-16', 8, 'Guardian Pharmacy', 8, 'Healthcare', 55.00, 3.30, 58.30, 'DebitCard', 'Recorded', 'Pharmacy supplies', NULL),
    ('RCP-2026-0009', '2026-05-05', 9, 'Golden Screen Cinemas', 9, 'Entertainment', 39.90, 2.39, 42.29, 'EWallet', 'Recorded', 'Movie night', NULL),
    ('RCP-2026-0010', '2026-05-10', 10, 'Udemy', 10, 'Education', 59.90, 3.59, 63.49, 'CreditCard', 'Reimbursed', 'Online programming course', NULL);
GO

INSERT INTO dbo.ReceiptManagementReceiptItems
    (ReceiptId, Description, Quantity, UnitPrice, LineTotal, Notes)
VALUES
    (1, 'Lunch set', 1, 18.90, 18.90, NULL),
    (1, 'Iced lemon tea', 1, 4.50, 4.50, NULL),
    (2, 'Ride fare', 1, 32.00, 32.00, 'Base ride charge'),
    (2, 'Toll surcharge', 1, 3.00, 3.00, NULL),
    (3, 'A4 paper pack', 2, 12.50, 25.00, NULL),
    (3, 'Blue pens', 10, 1.20, 12.00, NULL),
    (4, 'Rice 5kg', 1, 25.50, 25.50, NULL),
    (4, 'Egg tray', 1, 14.20, 14.20, NULL),
    (4, 'Milk carton', 1, 8.90, 8.90, NULL),
    (5, 'Mobile data plan', 1, 79.00, 79.00, NULL),
    (6, 'Cloud storage subscription', 1, 19.90, 19.90, NULL),
    (7, 'Flight ticket', 1, 188.00, 188.00, NULL),
    (7, 'Seat selection', 1, 20.00, 20.00, NULL),
    (8, 'Vitamin supplement', 1, 45.00, 45.00, NULL),
    (8, 'Face mask pack', 1, 10.00, 10.00, NULL),
    (9, 'Movie ticket', 1, 22.00, 22.00, NULL),
    (9, 'Popcorn combo', 1, 17.90, 17.90, NULL),
    (10, 'Programming course', 1, 59.90, 59.90, NULL),
    (3, 'Sticky notes', 2, 0.00, 0.00, 'Free promotion item'),
    (6, 'Platform discount adjustment', 1, 0.00, 0.00, 'Recorded for audit trail');
GO

SELECT 'ReceiptManagementDb created and seeded successfully.' AS ResultMessage;
SELECT COUNT(*) AS VendorCount FROM dbo.ReceiptManagementVendors;
SELECT COUNT(*) AS CategoryCount FROM dbo.ReceiptManagementExpenseCategories;
SELECT COUNT(*) AS ReceiptCount FROM dbo.ReceiptManagementReceipts;
SELECT COUNT(*) AS ReceiptItemCount FROM dbo.ReceiptManagementReceiptItems;
GO
