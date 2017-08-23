-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
-- SELECT dbo.AirTimeValid(46380, 21600, 86399, 120)
CREATE FUNCTION dbo.AirTimeValid 
(
	@air_time INT,
	@start_time INT,
	@end_time INT,
	@buffer_seconds INT
)
RETURNS BIT
AS
BEGIN
	DECLARE @return BIT
	DECLARE @new_start_time INT
	DECLARE @new_end_time INT

	SET @return = 0;

	-- everything is valid for 24hrs
	IF @start_time = 0 AND @end_time = 86399
		RETURN 1

	-- adjust start and end times by buffer taking into account special cases for start_time and end_time
	SET @new_start_time =	CASE WHEN @start_time - @buffer_seconds < 0		THEN 86400 - ABS(@start_time - @buffer_seconds) ELSE @start_time - @buffer_seconds	END
	SET @new_end_time =		CASE WHEN @end_time + @buffer_seconds > 86400	THEN ABS(86400 - (@end_time + @buffer_seconds))	ELSE @end_time + @buffer_seconds	END

	IF @new_end_time < @new_start_time
		BEGIN
			IF (@air_time BETWEEN @new_start_time AND 86400) OR (@air_time BETWEEN 0 AND @new_end_time)
				SET @return = 1
		END
	ELSE
		BEGIN
			IF @air_time BETWEEN @new_start_time AND @new_end_time
				SET @return = 1
		END
	
	
	RETURN @return;
END
