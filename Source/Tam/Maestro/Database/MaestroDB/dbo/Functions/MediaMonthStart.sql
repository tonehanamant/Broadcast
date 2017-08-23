CREATE FUNCTION [dbo].[MediaMonthStart] (@Period AS varchar(4))
RETURNS DATETIME
AS
BEGIN
	DECLARE	@FirstOfMonth AS DATETIME
	DECLARE @MediaMonthStart DATETIME
	
	SET @FirstOfMonth = dbo.Period2FirstOfMonth(@Period)

	IF (DATEPART(day,@FirstOfMonth) <> 1)
		RETURN NULL

	SET @MediaMonthStart = @FirstOfMonth
	WHILE DATENAME(weekday,@MediaMonthStart) <> 'Monday'
		SET @MediaMonthStart = DATEADD(day, -1, @MediaMonthStart)

	RETURN(@MediaMonthStart)
END
