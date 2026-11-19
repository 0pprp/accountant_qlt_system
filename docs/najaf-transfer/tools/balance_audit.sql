-- Run against NajafRehearsal (READ_ONLY). Formula from View_CustomersRemaining:
-- ROUND(sales - payments_by_CustomerIDPayment, -3)
SET NOCOUNT ON;

IF OBJECT_ID('tempdb..#Calc') IS NOT NULL DROP TABLE #Calc;

SELECT
  COUNT(*) AS Customers,
  SUM(CASE WHEN AmountRemaining > 0 THEN CAST(AmountRemaining AS decimal(28,2)) ELSE 0 END) AS Debts,
  SUM(CASE WHEN AmountRemaining < 0 THEN -CAST(AmountRemaining AS decimal(28,2)) ELSE 0 END) AS Credit,
  SUM(CAST(AmountRemaining AS decimal(28,2))) AS NetDisplayed
FROM dbo.View_CustomersRemaining;

SELECT
  (SELECT SUM(CAST(AmountTotalSalesDenar AS decimal(28,2))) FROM dbo.View_CustomersSales) AS SalesTotal,
  (SELECT SUM(CAST(AmountDenar AS decimal(28,2))) FROM dbo.View_AddToBox WHERE CustomerIDPayment IS NOT NULL) AS BoxAttributed,
  (SELECT SUM(CAST(AmountTotalSalesDenar AS decimal(28,2))) FROM dbo.View_CustomersSales)
    - (SELECT SUM(CAST(AmountDenar AS decimal(28,2))) FROM dbo.View_AddToBox WHERE CustomerIDPayment IS NOT NULL) AS RawNet;

;WITH Sales AS (
  SELECT CustomerID, SUM(CAST(AmountTotalSalesDenar AS decimal(28,4))) AS SalesAmt
  FROM dbo.View_CustomersSales
  GROUP BY CustomerID
),
Paid AS (
  SELECT CustomerIDPayment AS CustomerID, SUM(CAST(AmountDenar AS decimal(28,4))) AS PaidAmt
  FROM dbo.View_AddToBox
  WHERE CustomerIDPayment IS NOT NULL
  GROUP BY CustomerIDPayment
)
SELECT
  c.CustomerID,
  CAST(ISNULL(vr.AmountRemaining, 0) AS decimal(28,4)) AS Displayed,
  CAST(ISNULL(s.SalesAmt,0) - ISNULL(p.PaidAmt,0) AS decimal(28,4)) AS RawRemain,
  CAST(ROUND(ISNULL(s.SalesAmt,0) - ISNULL(p.PaidAmt,0), -3) AS decimal(28,4)) AS RoundedRemain
INTO #Calc
FROM dbo.Customers c
LEFT JOIN dbo.View_CustomersRemaining vr ON vr.CustomerID = c.CustomerID
LEFT JOIN Sales s ON s.CustomerID = c.CustomerID
LEFT JOIN Paid p ON p.CustomerID = c.CustomerID;

SELECT
  COUNT(*) AS AccountsDiffHalfDinar,
  SUM(CAST(RawRemain - Displayed AS decimal(28,2))) AS SumRawMinusDisplay,
  SUM(CAST(RoundedRemain - Displayed AS decimal(28,2))) AS SumRoundedMinusDisplay
FROM #Calc
WHERE ABS(Displayed - RoundedRemain) > 0.5
   OR ABS(Displayed - RawRemain) > 0.5;

SELECT TOP 30
  CustomerID, Displayed, RawRemain, RoundedRemain,
  ABS(Displayed - RoundedRemain) AS DiffVsRounded,
  ABS(Displayed - RawRemain) AS DiffVsRaw
FROM #Calc
WHERE ABS(Displayed - RoundedRemain) > 0.5
   OR ABS(Displayed - RawRemain) > 0.5
ORDER BY DiffVsRaw DESC;

-- Expected: SumRawMinusDisplay ≈ 542 for the whole population (not only diffs)
SELECT
  SUM(CAST(RawRemain AS decimal(28,2))) AS SumRaw,
  SUM(CAST(Displayed AS decimal(28,2))) AS SumDisplayed,
  SUM(CAST(RawRemain - Displayed AS decimal(28,2))) AS SumRawMinusDisplay_All
FROM #Calc;
