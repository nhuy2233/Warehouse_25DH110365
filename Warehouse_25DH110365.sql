CREATE DATABASE Warehouse_25DH110365;
GO
USE Warehouse_25DH110365;
GO
CREATE TABLE Products (
    ProductID   int IDENTITY(1,1) PRIMARY KEY,
    ProductName nvarchar(100) NOT NULL,
    Category    nvarchar(100) NOT NULL,
    Price       decimal(18,2) NOT NULL,
    Quantity    int NOT NULL
);
GO
INSERT INTO Products (ProductName, Category, Price, Quantity) VALUES
(N'Laptop Dell BuiNhutHuy',      N'Điện tử - BuiNhutHuy',          15000000, 10),
(N'Chuột không dây BuiNhutHuy',  N'Điện tử - BuiNhutHuy',          250000,   3),
(N'Bàn phím cơ BuiNhutHuy',      N'Điện tử - BuiNhutHuy',          900000,   0),
(N'Vở kẻ ngang BuiNhutHuy',      N'Văn phòng phẩm - BuiNhutHuy',   12000,    50),
(N'Bút bi BuiNhutHuy',           N'Văn phòng phẩm - BuiNhutHuy',   5000,     4),
(N'Kẹp giấy BuiNhutHuy',         N'Văn phòng phẩm - BuiNhutHuy',   8000,     5);
GO