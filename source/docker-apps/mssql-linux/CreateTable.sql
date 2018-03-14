USE TutorialDB
GO

-- Create a new table called 'Employees' in schema 'SchemaName'
-- Drop the table if it already exists
IF OBJECT_ID('dbo.Employees', 'U') IS NOT NULL
DROP TABLE SchemaName.Employees
GO
-- Create the table in the specified schema
CREATE TABLE dbo.Employees
(
    EmployeesId INT NOT NULL PRIMARY KEY, -- primary key column
    name [NVARCHAR](50) NOT NULL,
    location [NVARCHAR](50) NOT NULL
    -- specify more columns here
);
GO