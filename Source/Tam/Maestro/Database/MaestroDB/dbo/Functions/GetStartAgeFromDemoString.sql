CREATE FUNCTION [dbo].[GetStartAgeFromDemoString]
(@pDemoString NVARCHAR (100))
RETURNS INT
AS
 EXTERNAL NAME [SqlTest].[SqlTest.Functions].[GetStartAgeFromDemoString]

