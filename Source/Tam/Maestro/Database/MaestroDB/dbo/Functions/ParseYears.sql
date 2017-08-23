-- SELECT * FROM dbo.ParseYears('12/29/2008','3/29/2009')
CREATE FUNCTION [dbo].[ParseYears]
(	
	@start_date DATETIME,
	@end_date DATETIME
)
RETURNS @return TABLE 
(
	year INT
)
AS
BEGIN
	INSERT INTO @return
		SELECT 
			DISTINCT year 
		FROM 
			media_weeks
			JOIN media_months ON media_months.id=media_weeks.media_month_id
		WHERE
			(media_weeks.start_date <= @end_date AND media_weeks.end_date >= @start_date)

		RETURN;
END
