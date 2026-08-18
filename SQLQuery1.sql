CREATE DATABASE CRMDB

USE CRMDB;

CREATE TABLE Leads
(
    LeadId INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100),
    Phone NVARCHAR(15),
    Company NVARCHAR(100),
    Source NVARCHAR(50),
    Status NVARCHAR(30),
    LeadScore INT DEFAULT 0,
    CreatedDate DATETIME DEFAULT GETDATE()
);

INSERT INTO Leads
(FullName, Email, Phone, Company, Source, Status, LeadScore)
VALUES
('Rahul Sharma','rahul@gmail.com','9876543210','ABC Pvt Ltd','Website','New',75),
('Priya Patel','priya@gmail.com','9988776655','XYZ Ltd','Facebook','Contacted',82),
('Amit Kumar','amit@gmail.com','9123456780','Tech Solutions','Referral','Qualified',90);

CREATE TABLE Customers
(
    CustomerId INT PRIMARY KEY IDENTITY(1,1),
    LeadId INT,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100),
    Phone NVARCHAR(15),
    Company NVARCHAR(100),
    Address NVARCHAR(250),
    CreatedDate DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Customers_Leads
    FOREIGN KEY (LeadId)
    REFERENCES Leads(LeadId)
);

INSERT INTO Customers
(LeadId, FullName, Email, Phone, Company, Address)
VALUES
(3,'Amit Kumar','amit@gmail.com','9123456780','Tech Solutions','Pune');

CREATE TABLE Quotations
(
    QuotationId INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,
    QuoteNumber NVARCHAR(30),
    Amount DECIMAL(18,2),
    Description NVARCHAR(300),
    QuoteDate DATE,
    Status NVARCHAR(30),

    CONSTRAINT FK_Quotation_Customer
    FOREIGN KEY(CustomerId)
    REFERENCES Customers(CustomerId)
);

INSERT INTO Quotations
(CustomerId, QuoteNumber, Amount, Description, QuoteDate, Status)
VALUES
(1,'QT-1001',50000,'CRM Software',GETDATE(),'Pending');

CREATE TABLE FollowUps
(
    FollowUpId INT PRIMARY KEY IDENTITY(1,1),
    LeadId INT,
    CustomerId INT,
    FollowUpDate DATE,
    Notes NVARCHAR(500),
    Status NVARCHAR(30),

    CONSTRAINT FK_FollowUp_Lead
    FOREIGN KEY(LeadId)
    REFERENCES Leads(LeadId),

    CONSTRAINT FK_FollowUp_Customer
    FOREIGN KEY(CustomerId)
    REFERENCES Customers(CustomerId)
);

INSERT INTO FollowUps
(LeadId, CustomerId, FollowUpDate, Notes, Status)
VALUES
(1,NULL,DATEADD(DAY,3,GETDATE()),'Call customer regarding demo','Scheduled');

CREATE TABLE Sales
(
    SaleId INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,
    QuotationId INT,
    Amount DECIMAL(18,2),
    SaleDate DATE,
    PaymentStatus NVARCHAR(30),

    CONSTRAINT FK_Sales_Customer
    FOREIGN KEY(CustomerId)
    REFERENCES Customers(CustomerId),

    CONSTRAINT FK_Sales_Quotation
    FOREIGN KEY(QuotationId)
    REFERENCES Quotations(QuotationId)
);


INSERT INTO Sales
(CustomerId, QuotationId, Amount, SaleDate, PaymentStatus)
VALUES
(1,1,50000,GETDATE(),'Paid');

SELECT * FROM Leads;

SELECT * FROM Customers;

SELECT * FROM Quotations;

SELECT * FROM FollowUps;

SELECT * FROM Sales;


