
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/31/2013
-- Description:	Get's interecting days from two pairs of start/end dates.
-- =============================================
CREATE FUNCTION [dbo].[GetIntersectingDaysFromWeeks] 
(
	@start_date1 DATE,
	@end_date1 DATE,
	@start_date2 DATE,
	@end_date2 DATE
)
RETURNS VARCHAR(31)
AS
BEGIN
	DECLARE @return INT;
	SET @return = 0;

    DECLARE @min DATE
    DECLARE @max DATE
    
    SELECT
		@min = CASE WHEN @start_date1 < @start_date2 THEN @start_date1 ELSE @start_date2 END,
		@max = CASE WHEN @end_date1 < @end_date2 THEN @end_date1 ELSE @end_date2 END;
		
	DECLARE @current DATE;
	SET @current = @min;
	
	WHILE @current <= @max
	BEGIN
		IF @current BETWEEN @start_date1 AND @end_date1 AND @current BETWEEN @start_date2 AND @end_date2
			SET @return = @return + 1;
			
		SET @current = DATEADD(day,1,@current);
	END
		
	RETURN @return;
END
