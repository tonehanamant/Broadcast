CREATE FUNCTION [dbo].[MediaMonthEnd] (@Period AS varchar(4))
RETURNS DATETIME
AS
BEGIN
	DECLARE	@FirstOfMonth AS DATETIME
	DECLARE @MediaMonthEnd AS DATETIME
	
	SET @FirstOfMonth = dbo.Period2FirstOfMonth(@Period)

	SET @MediaMonthEnd = DATEADD(month, 1, @FirstOfMonth)
	SET @MediaMonthEnd = dbo.MediaMonthStart(dbo.FirstOfMonth2Period(@MediaMonthEnd))
	IF (@MediaMonthEnd IS NOT NULL)
		SET @MediaMonthEnd = DATEADD(day, -1, @MediaMonthEnd)

	RETURN(@MediaMonthEnd)
END
