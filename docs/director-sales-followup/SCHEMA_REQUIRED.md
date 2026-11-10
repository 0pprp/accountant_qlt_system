# SCHEMA_REQUIRED — SalesFollowup

يجب تطبيق الجداول التالية قبل تفعيل إيقاف التطبيق وتقييم المدير في الإنتاج.

## SalesEmployeeAppBlocks

```sql
CREATE TABLE dbo.SalesEmployeeAppBlocks (
    Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SalesEmployeeAppBlocks PRIMARY KEY,
    EmployeeUserId  INT NOT NULL,
    IsBlocked       BIT NOT NULL CONSTRAINT DF_SalesEmployeeAppBlocks_IsBlocked DEFAULT(0),
    UpdatedAtUtc    DATETIMEOFFSET NOT NULL,
    UpdatedByUserId INT NULL,
    UpdatedByName   NVARCHAR(200) NULL,
    CONSTRAINT UQ_SalesEmployeeAppBlocks_Employee UNIQUE (EmployeeUserId),
    CONSTRAINT FK_SalesEmployeeAppBlocks_User FOREIGN KEY (EmployeeUserId) REFERENCES dbo.Users(Id)
);
```

## DirectorSalesEmployeeBoardRatings

```sql
CREATE TABLE dbo.DirectorSalesEmployeeBoardRatings (
    Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DirectorSalesEmployeeBoardRatings PRIMARY KEY,
    EmployeeUserId  INT NOT NULL,
    Score           INT NOT NULL,
    Note            NVARCHAR(1000) NULL,
    RatedByUserId   INT NULL,
    RatedByUserName NVARCHAR(200) NULL,
    RatedAtUtc      DATETIMEOFFSET NOT NULL,
    CONSTRAINT FK_DirectorBoardRatings_Employee FOREIGN KEY (EmployeeUserId) REFERENCES dbo.Users(Id)
);

CREATE INDEX IX_DirectorBoardRatings_Employee_RatedAt
    ON dbo.DirectorSalesEmployeeBoardRatings (EmployeeUserId, RatedAtUtc DESC);
```

## اختياري لاحقًا

- عمود `Customers.IsLegal BIT NULL` لتطابق قاعدة المصفّر حرفيًا مع القديم.
