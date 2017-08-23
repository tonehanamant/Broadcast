-- SELECT * FROM dbo.ParseQuarters('12/29/2008','3/29/2009')
CREATE FUNCTION ParseQuarters
(	
	@start_date DATETIME,
	@end_date DATETIME
)
RETURNS @return TABLE 
(
	quarter INT
)
AS
BEGIN
	INSERT INTO @return
		SELECT 
			DISTINCT 
				CASE month 
					WHEN 1 THEN 1 
					WHEN 2 THEN 1 
					WHEN 3 THEN 1 
					WHEN 4 THEN 2
					WHEN 5 THEN 2
					WHEN 6 THEN 2
					WHEN 7 THEN 3
					WHEN 8 THEN 3
					WHEN 9 THEN 3
					WHEN 10 THEN 4
					WHEN 11 THEN 4
					WHEN 12 THEN 4
				END
		FROM 
			media_weeks
			JOIN media_months ON media_months.id=media_weeks.media_month_id
		WHERE
			(media_weeks.start_date <= @end_date AND media_weeks.end_date >= @start_date)

		RETURN;
END
