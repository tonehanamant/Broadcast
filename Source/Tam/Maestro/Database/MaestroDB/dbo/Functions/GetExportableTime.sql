-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 8/17/2015
-- Description:	Formats time in seconds as "hh:mm:ss tt".
--				Times greater than 86400 are reduced by 86400 and rendered.
-- =============================================
/*	TEST CASES:
	SELECT dbo.GetExportableTime(0)		'12:00:00 AM'
	SELECT dbo.GetExportableTime(86400) '12:00:00 AM'
	SELECT dbo.GetExportableTime(20)	'12:00:20 AM'
	SELECT dbo.GetExportableTime(3600)	'01:00:00 AM'
	SELECT dbo.GetExportableTime(35999) '09:59:59 AM'
	SELECT dbo.GetExportableTime(43199) '11:59:59 AM'
	SELECT dbo.GetExportableTime(43200) '12:00:00 PM'
	SELECT dbo.GetExportableTime(46800) '1:00:00 PM'
	SELECT dbo.GetExportableTime(86399) '11:59:59 PM'
	
	SELECT dbo.GetExportableTime(90000) '01:00:00 AM'
*/
CREATE FUNCTION [dbo].[GetExportableTime]
(
	@air_time INT
)
RETURNS VARCHAR(31)
AS
BEGIN
	DECLARE @return AS VARCHAR(31)
	
	DECLARE @hour INT
	DECLARE @minute INT
	DECLARE @second INT
	
	IF @air_time > 86400
		SET @air_time = @air_time - 86400;

	IF @air_time = 0 OR @air_time = 86400
		BEGIN
			SET @return = '12:00:00 AM';
		END
	ELSE
		BEGIN
			SET @hour = FLOOR(@air_time / 3600.0);
			SET @minute = (@air_time % 3600.0) / 60.0;
			SET @second = @air_time - ((@hour * 3600.0) + (@minute * 60.0));
					
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
				END + ':';
			SET @return = @return + 
				CASE
					WHEN @second < 10 THEN '0' + CAST(@second AS VARCHAR(2)) 
					ELSE CAST(@second AS VARCHAR(2)) 
				END;
			SET @return = @return + 
				CASE 
					WHEN @hour >= 12 THEN ' PM' 
					ELSE ' AM' 
				END;
		END

	RETURN @return;
END
