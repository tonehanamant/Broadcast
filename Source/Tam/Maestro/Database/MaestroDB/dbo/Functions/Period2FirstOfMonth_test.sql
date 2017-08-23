CREATE FUNCTION [dbo].[Period2FirstOfMonth_test] (@Period AS varchar(4))
RETURNS datetime 
WITH SCHEMABINDING
AS
BEGIN
	DECLARE @FirstOfMonth AS datetime

--	SET @FirstOfMonth = cast(SUBSTRING(@PERIOD,1 , 2) + '/01/20' + SUBSTRING(@PERIOD,3 , 2) as datetime)
	SET @FirstOfMonth = convert(datetime, SUBSTRING(@PERIOD,1 , 2) + '/01/20' + SUBSTRING(@PERIOD,3 , 2), 101)

	RETURN(@FirstOfMonth)
END
