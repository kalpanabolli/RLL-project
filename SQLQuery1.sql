CREATE DATABASE CarRentalEntities
USE CarRentalEntities

-- Create Customers Table with CustomerID as an auto-incrementing field
CREATE TABLE Customers (
    CustomerID INT PRIMARY KEY IDENTITY(1,1),
    CustomerName NVARCHAR(255) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    Password NVARCHAR(255) NOT NULL,
    LoyaltyPoints INT DEFAULT 0
);

-- Create Cars Table with CarID as an auto-incrementing field
CREATE TABLE Cars (
    CarID INT PRIMARY KEY IDENTITY(1,1),
    CarName NVARCHAR(255) NOT NULL,
    Available BIT NOT NULL,
    PerDayCharge DECIMAL(10, 2) NOT NULL,
    ChargePerKm DECIMAL(10, 2) NOT NULL,
    CarType NVARCHAR(255) NOT NULL,
    Photo NVARCHAR(255)
);

-- Create Rentals Table with RentID as an auto-incrementing field
CREATE TABLE Rentals (
    RentID INT PRIMARY KEY IDENTITY(1,1),
    CustomerID INT NOT NULL,
    CarID INT NOT NULL,
    RentOrderDate DATE NOT NULL,
    ReturnDate DATE NOT NULL,
    OdoReading INT,
    ReturnOdoReading INT,
    LicenseNumber NVARCHAR(255) NOT NULL,
    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
    FOREIGN KEY (CarID) REFERENCES Cars(CarID)
);

-- Create Cost Table with CostID as an auto-incrementing field
CREATE TABLE Cost (
    CostID INT PRIMARY KEY IDENTITY(1,1),
    RentID INT NOT NULL,
    KmsCovered INT NOT NULL,
    Price DECIMAL(10, 2) NOT NULL,
    Tax DECIMAL(10, 2),
    TotalCost DECIMAL(10, 2) NOT NULL,
    FOREIGN KEY (RentID) REFERENCES Rentals(RentID)
);

-- Create Admin Table
CREATE TABLE admin(
    Username VARCHAR(50) PRIMARY KEY,
    Password VARCHAR(50)
);

-- Insert a record into Admin Table
INSERT INTO admin VALUES ('admin@gmail.com','Admin@123');

select * from Customers