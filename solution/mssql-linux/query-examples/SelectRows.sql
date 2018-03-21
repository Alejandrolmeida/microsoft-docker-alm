USE TutorialDB
GO

-- Query the total count of employees
SELECT COUNT(*) as EmployeeCount FROM dbo.Employees;
-- Query all employee information
SELECT e.EmployeesId, e.Name, e.Location 
FROM dbo.Employees as e
GO