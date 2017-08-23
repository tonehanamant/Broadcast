create FUNCTION [dbo].[Period2FirstOfMonth] (@Period AS varchar(4))
RETURNS datetime
AS
BEGIN
	DECLARE @FirstOfMonth AS datetime
	
	SET @FirstOfMonth = CAST(SUBSTRING(@PERIOD,1 , 2) + '/01/' + SUBSTRING(@PERIOD,3 , 2) as datetime)

	RETURN(@FirstOfMonth)
END
