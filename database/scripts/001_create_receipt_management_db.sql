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
    ('ZUS Coffee - XMUM', 'Cafe Counter', '+6012-816-1340', NULL, 'A3 Library Cafe, Xiamen University Malaysia, Jalan Sunsuria, Bandar Sunsuria, 43900 Sepang, Selangor', 'XMUM-ZUS-001', 'On-campus coffee and pastry purchases'),
    ('D6 Cafeteria XMUM', 'Cafeteria Desk', '+6012-473-8165', NULL, 'Block D6, Xiamen University Malaysia, Jalan Sunsuria, Bandar Sunsuria, 43900 Sepang, Selangor', 'XMUM-D6-002', 'Campus cafeteria meals'),
    ('myNEWS XMUM', 'Store Counter', NULL, NULL, 'Block D6, Ground Floor, Xiamen University Malaysia, Jalan Sunsuria, Bandar Sunsuria, 43900 Sepang, Selangor', 'XMUM-MYNEWS-003', 'Campus convenience store and stationery'),
    ('Good Taste Xiamen University', 'Restaurant Desk', NULL, NULL, 'LY3-104, Xiamen University Malaysia, Jalan Sunsuria, Bandar Sunsuria, 43900 Sepang, Selangor', 'XMUM-GT-004', 'Nearby student meals'),
    ('LakeFront Cafe - XMU', 'Cafe Counter', NULL, NULL, 'Xiamen University Malaysia lakeside area, Jalan Sunsuria, Bandar Sunsuria, 43900 Sepang, Selangor', 'XMUM-LAKE-005', 'Campus cafe and group study meals'),
    ('Rayse Nourish Bell Avenue', 'Service Counter', NULL, NULL, 'R-1-13A Bell Avenue, Jalan Sunsuria, Bandar Sunsuria City, 43900 Sepang, Selangor', 'SUNS-RAYSE-006', 'Healthy meal boxes near XMUM'),
    ('AllAce Bell Avenue Sunsuria', 'Restaurant Counter', '+6011-1084-4801', NULL, 'R-G-21, R1-21, Bell Avenue, Jalan Sunsuria, Bandar Sunsuria City, 43900 Sepang, Selangor', 'SUNS-ALLACE-007', 'Fast food near campus'),
    ('Secret Recipe Bell Avenue', 'Front Counter', NULL, NULL, 'R-G-33, Bell Avenue, Jalan Sunsuria, Bandar Sunsuria, 43900 Sepang, Selangor', 'SUNS-SR-008', 'Dessert and Western food near XMUM'),
    ('The Daily Kueh Bell Avenue', 'Bakery Counter', '+6012-907-2861', NULL, 'R-G-39, Bell Avenue, Jalan Sunsuria, Bandar Sunsuria, 43900 Sepang, Selangor', 'SUNS-KUEH-009', 'Local kuih and bakery items near campus'),
    ('HeyCha Bell Avenue', 'Tea Bar Counter', NULL, NULL, 'R-G-32, Bell Avenue, Jalan Sunsuria, Bandar Sunsuria, 43900 Sepang, Selangor', 'SUNS-HEYCHA-010', 'Tea drinks near campus');
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
    ('RCP-2026-0001', '2026-01-10', 1, 'ZUS Coffee - XMUM', 1, 'Food & Dining', 31.70, 1.90, 33.60, 'EWallet', 'Recorded', 'Coffee stop at A3 Library Cafe before lecture', '/uploads/receipts/seed/rcp-2026-0001.png'),
    ('RCP-2026-0002', '2026-01-22', 2, 'D6 Cafeteria XMUM', 1, 'Food & Dining', 28.50, 0.00, 28.50, 'EWallet', 'Recorded', 'Campus lunch with multiple cafeteria items', '/uploads/receipts/seed/rcp-2026-0002.png'),
    ('RCP-2026-0003', '2026-02-04', 3, 'myNEWS XMUM', 3, 'Office Supplies', 50.20, 3.01, 53.21, 'DebitCard', 'Recorded', 'Convenience and stationery run inside campus', '/uploads/receipts/seed/rcp-2026-0003.png'),
    ('RCP-2026-0004', '2026-02-18', 4, 'Good Taste Xiamen University', 1, 'Food & Dining', 23.80, 0.00, 23.80, 'Cash', 'Recorded', 'Dinner near hostel after evening class', '/uploads/receipts/seed/rcp-2026-0004.png'),
    ('RCP-2026-0005', '2026-03-08', 5, 'LakeFront Cafe - XMU', 1, 'Food & Dining', 42.80, 2.57, 45.37, 'EWallet', 'Recorded', 'Group study meal near the campus lakefront', '/uploads/receipts/seed/rcp-2026-0005.png'),
    ('RCP-2026-0006', '2026-03-21', 6, 'Rayse Nourish Bell Avenue', 1, 'Food & Dining', 43.80, 2.63, 46.43, 'CreditCard', 'Recorded', 'Healthy meal boxes from Bell Avenue', '/uploads/receipts/seed/rcp-2026-0006.png'),
    ('RCP-2026-0007', '2026-04-02', 7, 'AllAce Bell Avenue Sunsuria', 1, 'Food & Dining', 32.50, 1.95, 34.45, 'EWallet', 'Draft', 'Fast food after lab session', '/uploads/receipts/seed/rcp-2026-0007.png'),
    ('RCP-2026-0008', '2026-04-16', 8, 'Secret Recipe Bell Avenue', 1, 'Food & Dining', 69.60, 4.18, 73.78, 'DebitCard', 'Recorded', 'Dessert and Western food near campus', '/uploads/receipts/seed/rcp-2026-0008.png'),
    ('RCP-2026-0009', '2026-05-05', 9, 'The Daily Kueh Bell Avenue', 1, 'Food & Dining', 30.50, 1.83, 32.33, 'EWallet', 'Recorded', 'Local kuih for group meeting snack', '/uploads/receipts/seed/rcp-2026-0009.png'),
    ('RCP-2026-0010', '2026-05-10', 10, 'HeyCha Bell Avenue', 1, 'Food & Dining', 31.00, 1.86, 32.86, 'CreditCard', 'Reimbursed', 'Tea drinks near campus after presentation', '/uploads/receipts/seed/rcp-2026-0010.png');
GO

INSERT INTO dbo.ReceiptManagementReceiptItems
    (ReceiptId, Description, Quantity, UnitPrice, LineTotal, Notes)
VALUES
    (1, 'Spanish Latte', 1, 11.20, 11.20, NULL),
    (1, 'Hot Latte', 1, 10.20, 10.20, NULL),
    (1, 'Banana bread slice', 1, 6.50, 6.50, NULL),
    (1, 'Extra espresso shot', 1, 2.00, 2.00, NULL),
    (1, 'Gula Melaka syrup add-on', 1, 1.80, 1.80, NULL),
    (2, 'Mixed rice base', 1, 8.50, 8.50, NULL),
    (2, 'Chicken dish add-on', 1, 9.50, 9.50, NULL),
    (2, 'Vegetable side', 1, 4.00, 4.00, NULL),
    (2, 'Fried egg', 1, 2.00, 2.00, NULL),
    (2, 'Iced tea', 1, 2.50, 2.50, NULL),
    (2, 'Takeaway box', 1, 2.00, 2.00, NULL),
    (3, 'Mineral water', 2, 1.50, 3.00, NULL),
    (3, 'Chicken sandwich', 1, 7.90, 7.90, NULL),
    (3, 'A5 notebook', 2, 5.90, 11.80, NULL),
    (3, 'Black pen pack', 1, 6.50, 6.50, NULL),
    (3, 'USB-C cable', 1, 12.00, 12.00, NULL),
    (3, 'Pocket tissue pack', 1, 3.20, 3.20, NULL),
    (3, 'Instant noodles', 3, 1.60, 4.80, NULL),
    (3, 'Checkout bag', 1, 1.00, 1.00, NULL),
    (4, 'Claypot noodles', 1, 9.80, 9.80, NULL),
    (4, 'Lemon tea', 1, 2.50, 2.50, NULL),
    (4, 'Fried dumplings', 1, 6.00, 6.00, NULL),
    (4, 'Rice add-on', 1, 1.50, 1.50, NULL),
    (4, 'Curry puff', 1, 4.00, 4.00, NULL),
    (5, 'Chicken chop', 1, 17.90, 17.90, NULL),
    (5, 'Iced Americano', 1, 8.90, 8.90, NULL),
    (5, 'French fries', 1, 6.50, 6.50, NULL),
    (5, 'Mushroom soup', 1, 7.50, 7.50, NULL),
    (5, 'Mineral water', 1, 2.00, 2.00, NULL),
    (6, 'Butter chicken with rice', 1, 9.90, 9.90, NULL),
    (6, 'Egg mayo sandwich', 1, 7.90, 7.90, NULL),
    (6, 'Sunshine pasta salad', 1, 12.50, 12.50, NULL),
    (6, 'Chicken skewer mango salsa', 1, 13.50, 13.50, NULL),
    (7, 'Chicken burger', 1, 11.90, 11.90, NULL),
    (7, 'Fries', 1, 5.50, 5.50, NULL),
    (7, 'Crispy chicken', 1, 9.90, 9.90, NULL),
    (7, 'Cola', 1, 3.20, 3.20, NULL),
    (7, 'Cheese dip', 1, 2.00, 2.00, NULL),
    (8, 'Lunch set', 1, 18.90, 18.90, NULL),
    (8, 'Iced lemon tea', 1, 5.50, 5.50, NULL),
    (8, 'Chocolate indulgence slice', 1, 13.90, 13.90, NULL),
    (8, 'Mushroom soup', 1, 8.90, 8.90, NULL),
    (8, 'Grilled chicken chop', 1, 21.90, 21.90, NULL),
    (8, 'Takeaway bag', 1, 0.50, 0.50, NULL),
    (9, 'Onde-onde pack', 1, 6.50, 6.50, NULL),
    (9, 'Kuih lapis', 1, 7.00, 7.00, NULL),
    (9, 'Curry puff', 1, 5.00, 5.00, NULL),
    (9, 'Pandan cake slice', 1, 8.50, 8.50, NULL),
    (9, 'Teh tarik', 1, 3.50, 3.50, NULL),
    (10, 'Brown sugar milk tea', 1, 10.90, 10.90, NULL),
    (10, 'Jasmine tea', 1, 7.90, 7.90, NULL),
    (10, 'Pearl topping', 1, 1.50, 1.50, NULL),
    (10, 'Cheese foam', 1, 2.50, 2.50, NULL),
    (10, 'Mango tea', 1, 8.20, 8.20, NULL);
GO

SELECT 'ReceiptManagementDb created and seeded successfully.' AS ResultMessage;
SELECT COUNT(*) AS VendorCount FROM dbo.ReceiptManagementVendors;
SELECT COUNT(*) AS CategoryCount FROM dbo.ReceiptManagementExpenseCategories;
SELECT COUNT(*) AS ReceiptCount FROM dbo.ReceiptManagementReceipts;
SELECT COUNT(*) AS ReceiptItemCount FROM dbo.ReceiptManagementReceiptItems;
GO
