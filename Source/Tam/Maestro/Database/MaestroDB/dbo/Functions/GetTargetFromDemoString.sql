CREATE FUNCTION [dbo].[GetTargetFromDemoString]
(@pDemoString NVARCHAR (100))
RETURNS NVARCHAR (100)
AS
 EXTERNAL NAME [SqlTest].[SqlTest.Functions].[GetTargetFromDemoString]

