-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/23/2009
-- Modified:	8/17/2015 - Stephen DeFusco - Fixed issue with 12:00AM to 12:59:59AM rendering and times greater than 86400.
-- Description:	Formats time in seconds as "hh:mm tt" effectively truncating the seconds if there are any.
--				Times greater than 86400 are reduced by 86400 and rendered.
-- =============================================
/*	TEST CASES:
	SELECT dbo.GetReadableTime(0)		'12:00am'
	SELECT dbo.GetReadableTime(86400)	'12:00am'
	SELECT dbo.GetReadableTime(20)		'12:00am'
	SELECT dbo.GetReadableTime(3600)	'01:00am'
	SELECT dbo.GetReadableTime(35999)	'09:59am'
	SELECT dbo.GetReadableTime(43199)	'11:59am'
	SELECT dbo.GetReadableTime(43200)	'12:00pm'
	SELECT dbo.GetReadableTime(46800)	'1:00pm'
	SELECT dbo.GetReadableTime(86399)	'11:59pm'
	
	SELECT dbo.GetReadableTime(90000)	'01:00am'
*/
CREATE FUNCTION [dbo].[GetReadableTime]
(
	@air_time INT
)
RETURNS VARCHAR(31)
AS
BEGIN
	DECLARE @return AS VARCHAR(31)
	
	DECLARE @hour INT;
	DECLARE @minute INT;
	
	IF @air_time > 86400
		SET @air_time = @air_time - 86400;

	SET @hour = FLOOR(@air_time / 3600.0);
	SET @minute = (@air_time % 3600.0) / 60.0;

	IF @air_time = 0 OR @air_time = 86400
		BEGIN
			SET @return = '12:00am';
		END
	ELSE
		BEGIN
			SET @return = 
				CASE 
					WHEN @hour=0 THEN '12' 
					WHEN @hour < 10 THEN '0' + CAST(@hour AS VARCHAR(2)) 
					WHEN @hour > 12 THEN CAST(@hour-12 AS VARCHAR) 
					ELSE CAST(@hour AS VARCHAR(2)) 
				END + ':';
			SET @return = @return + 
				CASE 
					WHEN @minute < 10 THEN '0' + CAST(@minute AS VARCHAR(2)) 
					ELSE CAST(@minute AS VARCHAR(2)) 
				END;
			SET @return = @return + 
				CASE 
					WHEN @hour >= 12 THEN 'pm' 
					ELSE 'am' 
				END;
		END

	RETURN @return;
END

