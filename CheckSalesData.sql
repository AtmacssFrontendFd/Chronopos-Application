-- Check Sales Data in Database
-- Run this in your database to see if you have sales records

-- Count of all sales
SELECT 'Total Sales Count' as Info, COUNT(*) as Count FROM Sales;

-- Sales by status
SELECT 
    Status,
    COUNT(*) as Count,
    SUM(TotalAmount) as TotalAmount
FROM Sales
GROUP BY Status;

-- Recent sales (last 10)
SELECT TOP 10
    Id,
    TransactionNumber,
    SaleDate,
    TotalAmount,
    Status,
    PaymentMethod
FROM Sales
ORDER BY SaleDate DESC;

-- Sales for today
SELECT 
    'Today Sales' as Info,
    COUNT(*) as Count,
    SUM(TotalAmount) as TotalAmount
FROM Sales
WHERE CAST(SaleDate AS DATE) = CAST(GETDATE() AS DATE);

-- Sales for last 7 days
SELECT 
    'Last 7 Days' as Info,
    COUNT(*) as Count,
    SUM(TotalAmount) as TotalAmount
FROM Sales
WHERE SaleDate >= DATEADD(day, -7, GETDATE());

-- Sales with items
SELECT 
    s.Id,
    s.TransactionNumber,
    s.SaleDate,
    s.TotalAmount,
    s.Status,
    COUNT(si.Id) as ItemCount
FROM Sales s
LEFT JOIN SaleItems si ON s.Id = si.SaleId
GROUP BY s.Id, s.TransactionNumber, s.SaleDate, s.TotalAmount, s.Status
ORDER BY s.SaleDate DESC;
