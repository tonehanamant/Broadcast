-- =============================================
-- Author:		<Nick, Kheynis>
-- Create date: <2/25/14>
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetDemoRankersReport 387, 390, 5, 1, 1, 1, 247, 410, 1, '12/30/2013', '3/30/2014', 21600, 43199, 1, 1, 1, 1, 1, 0, 0
CREATE PROCEDURE [dbo].[usp_PCS_GetDemoRankersReport]
	@base_rating_media_month_id INT,
	@base_universe_media_month_id INT,
	@sales_model_id INT,
	@spot_length_id INT,
	@rate_card_type_id INT,
	@rating_source_id TINYINT,
	@audience_id INT,
	@network_rate_card_book_id INT,
	@rate_card_daypart_id INT,
	@start_date DATETIME,
	@end_date DATETIME,
	@start_time INT,
	@end_time INT,
	@mon BIT,
	@tue BIT,
	@wed BIT,
	@thu BIT,
	@fri BIT,
	@sat BIT,
	@sun BIT
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @network_id INT;
	DECLARE @hiatus_weeks FlightTable;
					
	CREATE TABLE #networks (network_id INT, network VARCHAR(63))
	INSERT INTO #networks
		SELECT DISTINCT
			n.id,
			n.code
		FROM
			network_rate_card_books nrcb (NOLOCK)
			JOIN network_rate_cards nrc (NOLOCK) ON nrc.network_rate_card_book_id = nrcb.id
				AND nrc.rate_card_type_id = @rate_card_type_id
				AND nrc.daypart_id = 1
			JOIN network_rate_card_details nrcd (NOLOCK) ON nrcd.network_rate_card_id = nrc.id
			JOIN network_rate_card_rates nrcr (NOLOCK) ON nrcr.network_rate_card_detail_id = nrcd.id
				AND nrcr.spot_length_id = @spot_length_id
			JOIN networks n (NOLOCK) ON n.id = nrcd.network_id
		WHERE
			nrcb.id = @network_rate_card_book_id;
	
	CREATE TABLE #universes (network_id INT, universe FLOAT)
	INSERT INTO #universes
		SELECT
			n.network_id,
			sum(cud.universe)
		FROM
			coverage_universes cu (NOLOCK)
			JOIN coverage_universe_details cud (NOLOCK) ON cud.coverage_universe_id = cu.id
			JOIN #networks n ON n.network_id = cud.network_id
			JOIN network_rate_card_book_topographies nrct (NOLOCK) ON nrct.network_rate_card_book_id = @network_rate_card_book_id
				AND nrct.topography_id = cud.topography_id
		WHERE
			cu.base_media_month_id = @base_universe_media_month_id
			AND cu.sales_model_id = @sales_model_id
		GROUP BY
			n.network_id
		
	CREATE TABLE #forecast_months (media_month_id INT, num_weeks FLOAT)
	INSERT INTO #forecast_months
		SELECT mm.id,COUNT(1) FROM media_months mm (NOLOCK) JOIN media_weeks mw (NOLOCK) ON mw.media_month_id=mm.id WHERE mm.start_date <= @end_date AND mm.end_date >= @start_date GROUP BY mm.id

	CREATE TABLE #output (network VARCHAR(63), hh_cvg_universe INT, rate_card_rate MONEY, hh_rating FLOAT, hh_delivery FLOAT, hh_cpm MONEY, vpvh FLOAT, demo_rating FLOAT, demo_delivery FLOAT, demo_cpm MONEY)

	DECLARE NetworkCursor CURSOR FAST_FORWARD FOR
		SELECT network_id FROM #networks

	OPEN NetworkCursor
	FETCH NEXT FROM NetworkCursor INTO @network_id
	WHILE @@FETCH_STATUS = 0
		BEGIN
			INSERT INTO #output
				SELECT
					tmp.network,
					tmp.hh_cvg_universe,
					ISNULL(dbo.udf_GetRateCardRate(@network_rate_card_book_id,@rate_card_type_id,@network_id,@rate_card_daypart_id,@start_date,@spot_length_id,tmp.hh_delivery,@start_time,@end_time,@mon,@tue,@wed,@thu,@fri,@sat,@sun), 0) 'rate_card_rate',
					tmp.hh_rating * 100.0,
					tmp.hh_delivery / 1000.0,
					NULL 'hh_cpm',
					CASE WHEN tmp.hh_delivery > 0 THEN
						tmp.demo_delivery / tmp.hh_delivery
					ELSE
						NULL
					END 'vpvh',
					tmp.demo_rating * 100.0,
					tmp.demo_delivery / 1000.0,
					NULL 'demo_cpm'
				FROM (
					SELECT
						n.network,
						n.network_id,
						u.universe 'hh_cvg_universe',
						r_hh.rating 'hh_rating',
						(r_hh.rating * r_hh.us_universe * (u.universe / r_hh.us_universe)) 'hh_delivery',
						r_dm.rating 'demo_rating',
						(r_dm.rating * r_dm.us_universe * (u.universe / r_hh.us_universe)) 'demo_delivery'
					FROM
						#networks n
						JOIN #universes u ON u.network_id=n.network_id
						CROSS APPLY dbo.udf_GetCustomRatings(@network_id,31,@base_rating_media_month_id,@start_date,@end_date,@start_time,@end_time,@mon,@tue,@wed,@thu,@fri,@sat,@sun,0,@rating_source_id,@hiatus_weeks,NULL) r_hh
						CROSS APPLY dbo.udf_GetCustomRatings(@network_id,@audience_id,@base_rating_media_month_id,@start_date,@end_date,@start_time,@end_time,@mon,@tue,@wed,@thu,@fri,@sat,@sun,0,@rating_source_id,@hiatus_weeks,NULL) r_dm
					WHERE
						n.network_id=@network_id
				) tmp
					
			FETCH NEXT FROM NetworkCursor INTO @network_id
		END
	CLOSE NetworkCursor
	DEALLOCATE NetworkCursor
	
	UPDATE
		#output
	SET
		hh_cpm = CASE WHEN hh_delivery > 0 THEN rate_card_rate / hh_delivery ELSE NULL END,
		demo_cpm = CASE WHEN demo_delivery > 0 THEN rate_card_rate / demo_delivery ELSE NULL END
		
	SELECT * FROM #output ORDER BY network
		
	DROP TABLE #networks;
	DROP TABLE #universes;
	DROP TABLE #output;
	DROP TABLE #forecast_months;
END
