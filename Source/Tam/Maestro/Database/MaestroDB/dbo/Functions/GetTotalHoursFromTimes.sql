-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/1/2014
-- Description:	Function automatically adds one second to the end time (since we store and process the end_time (-1 second))
-- =============================================
-- SELECT dbo.GetTotalHoursFromTimes(21600, 86399)
CREATE FUNCTION [dbo].[GetTotalHoursFromTimes]
(
	@start_time INT,
	@end_time INT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @return FLOAT;

	SET @return =
		CASE 
			WHEN @end_time<@start_time THEN 
				-- overnight
				(((@end_time+86401)-@start_time))/3600.0
			ELSE 
				-- non-overnight
				(((@end_time+1)-@start_time))/3600.0
		END;
	
	RETURN @return;
END
