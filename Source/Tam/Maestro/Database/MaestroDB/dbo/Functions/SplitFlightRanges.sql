-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 12/10/2014
-- Description:	Accepts a string based comma separated list of start/end date combos outputs a table of media week ids
-- Sample:		SELECT * FROM dbo.SplitFlightRanges('12/1/2014-12/7/2014,12/8/2014-12/14/2014,12/22/2014-12/28/2014')
-- =============================================
CREATE FUNCTION [dbo].[SplitFlightRanges]
(
	@weeks VARCHAR(MAX)
)
RETURNS 
@return TABLE
(
	media_week_id INT
)
AS
BEGIN
	DECLARE 
		@flight VARCHAR(MAX), 
		@start_date VARCHAR(MAX), 
		@end_date VARCHAR(MAX), 
		@pos INT

	SET @weeks = LTRIM(RTRIM(@weeks)) + ','
	SET @pos = CHARINDEX(',', @weeks, 1)

	IF REPLACE(@weeks, ',', '') <> ''
		BEGIN
			WHILE @pos > 0
				BEGIN
					SET @flight = LTRIM(RTRIM(LEFT(@weeks, @pos - 1)))
					IF @flight <> ''
						BEGIN
							INSERT INTO @return
								SELECT DISTINCT
									mw.id
								FROM
									dbo.SplitDateTimes(@flight,'-') dt
									JOIN dbo.media_weeks mw (NOLOCK) ON dt.id BETWEEN mw.start_date AND mw.end_date
								WHERE
									mw.id NOT IN (
										SELECT media_week_id FROM @return
									)
						END
					SET @weeks = RIGHT(@weeks, LEN(@weeks) - @pos)
					SET @pos = CHARINDEX(',', @weeks, 1)
				END
		END	
	RETURN
END
