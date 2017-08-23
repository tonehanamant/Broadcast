-- usp_RCS_SearchNetworkRateCards 2,1,1,NULL,NULL
CREATE PROCEDURE [dbo].[usp_RCS_SearchNetworkRateCards]
	@sales_model_id INT,
	@daypart_id INT,
	@rate_card_type_id INT,
	@start_date DATETIME,	-- optional
	@end_date DATETIME		-- optional
AS
BEGIN
	CREATE TABLE #tmp (network_rate_card_id INT, network_rate_card_book_id INT, name VARCHAR(63), base_ratings_media_month_id INT, base_coverage_universe_media_month_id INT, daypart_text VARCHAR(63), daypart_id INT, rate_card_type_id INT, year INT, quarter INT, version INT, sales_model_id INT, start_date DATETIME, end_date DATETIME)
	INSERT INTO #tmp
		SELECT DISTINCT
			nrc.id,
			nrcb.id,
			nrcb.name,
			nrcb.base_ratings_media_month_id,
			nrcb.base_coverage_universe_media_month_id,
			d.daypart_text,
			d.id,
			nrc.rate_card_type_id,
			nrcb.year,
			nrcb.quarter,
			nrcb.version,
			nrcb.sales_model_id,
			MIN(mm.start_date) 'start_date',
			MAX(mm.end_date) 'end_date'
		FROM
			network_rate_cards nrc (NOLOCK)
			JOIN network_rate_card_books nrcb (NOLOCK) ON nrcb.id=nrc.network_rate_card_book_id
			JOIN media_months mm (NOLOCK) ON nrcb.year=mm.year 
				AND nrcb.quarter=CASE mm.month WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END
			JOIN dayparts d (NOLOCK) ON d.id=nrc.daypart_id
			JOIN sales_models sm (NOLOCK) ON sm.id=nrcb.sales_model_id
		WHERE
			date_approved IS NOT NULL
			AND sales_model_id=@sales_model_id
			AND nrc.daypart_id=@daypart_id
			AND nrc.rate_card_type_id=@rate_card_type_id
		GROUP BY
			nrc.id,
			nrcb.id,
			nrcb.name,
			nrcb.base_ratings_media_month_id,
			nrcb.base_coverage_universe_media_month_id,
			d.daypart_text,
			d.id,
			nrc.rate_card_type_id,
			nrcb.year,
			nrcb.quarter,
			nrcb.version,
			nrcb.sales_model_id
		ORDER BY
			nrcb.year DESC,
			nrcb.quarter DESC,
			nrcb.version DESC
			
	IF @start_date IS NULL AND @end_date IS NULL
		SELECT * FROM #tmp
	ELSE IF @start_date IS NOT NULL AND @end_date IS NULL
		SELECT * FROM #tmp WHERE (@start_date BETWEEN start_date AND end_date)
	ELSE IF @start_date IS NULL AND @end_date IS NOT NULL
		SELECT * FROM #tmp WHERE (@end_date BETWEEN start_date AND end_date)
	ELSE
		SELECT * FROM #tmp WHERE (start_date <= @end_date AND end_date >= @start_date)
		
	DROP TABLE #tmp;
END
