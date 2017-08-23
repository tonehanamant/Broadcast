-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/5/2009
-- Modified:	8/17/2015 - Stephen DeFusco - Fixed issue with times greater than and equal to 86400.
-- Description:	Formats time in seconds as "hh:mm:ss" and is in military time.
-- =============================================
/*	TEST CASES:
	SELECT dbo.GetLongReadableTime(0)		'00:00:00'
	SELECT dbo.GetLongReadableTime(86400)	'00:00:00'
	SELECT dbo.GetLongReadableTime(20)		'00:00:20'
	SELECT dbo.GetLongReadableTime(3600)	'01:00:00'
	SELECT dbo.GetLongReadableTime(35999)	'09:59:59'
	SELECT dbo.GetLongReadableTime(43199)	'11:59:59'
	SELECT dbo.GetLongReadableTime(43200)	'12:00:00'
	SELECT dbo.GetLongReadableTime(46800)	'13:00:00'
	SELECT dbo.GetLongReadableTime(86399)	'23:59:59'
	
	SELECT dbo.GetLongReadableTime(90000)	'01:00:00'
*/
CREATE FUNCTION [dbo].[GetLongReadableTime]
(
	@air_time INT
)
RETURNS VARCHAR(15)
AS
BEGIN
	DECLARE @return AS VARCHAR(15)
	
	DECLARE @hour INT
	DECLARE @minute INT
	DECLARE @second INT
	
	IF @air_time > 86400
		SET @air_time = @air_time - 86400;

	SET @hour = FLOOR(@air_time / 3600.0);
	SET @minute = (@air_time % 3600.0) / 60.0;
	SET @second = @air_time - ((@hour * 3600.0) + (@minute * 60.0))


	IF @air_time = 0 OR @air_time = 86400
		BEGIN
			SET @return = '00:00:00';
		END
	ELSE
		BEGIN
			SET @return = CASE WHEN @hour < 10 THEN '0' + CAST(@hour AS VARCHAR(2)) ELSE CAST(@hour AS VARCHAR(2)) END + ':'
			SET @return = @return + CASE WHEN @minute < 10 THEN '0' + CAST(@minute AS VARCHAR(2)) ELSE CAST(@minute AS VARCHAR(2)) END + ':'
			SET @return = @return + CASE WHEN @second < 10 THEN '0' + CAST(@second AS VARCHAR(2)) ELSE CAST(@second AS VARCHAR(2)) END
		END

	RETURN @return;
END

