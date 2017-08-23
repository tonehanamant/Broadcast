-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 2/25/2014
-- Description:	Calculates a rate card rate given the required parameters for a sales model.
-- =============================================
-- SELECT dbo.udf_GetRateCardRatev2(406,1,1,NULL,'2/21/2014',1,460124,21600,86399,1,1,1,1,1,1,1)
-- SELECT dbo.udf_GetRateCardRatev2(410,1,1,1,'3/7/2014',1,460124,21600,86399,1,1,1,1,1,1,1)
CREATE FUNCTION [dbo].[udf_GetRateCardRatev2]
(
	@network_rate_card_book_id INT,
	@rate_card_type_id TINYINT,
	@network_id INT,
	@rate_card_daypart_id INT, -- not required for sales models 5,6 (IMW, TAM Cable)
	@effective_date DATE,
	@spot_length_id INT, -- not required for sales models 5,6 (IMW, TAM Cable)
	@hh_delivery FLOAT, -- NOT IN (000)!, required for sales models 5,6 (IMW, TAM Cable)
	-- reprsents ratings daypart
	@start_time INT,	-- required for sales models 5,6 (IMW, TAM Cable)
	@end_time INT,		-- required for sales models 5,6 (IMW, TAM Cable)
	@mon BIT,			-- required for sales models 5,6 (IMW, TAM Cable)
	@tue BIT,			-- required for sales models 5,6 (IMW, TAM Cable)
	@wed BIT,			-- required for sales models 5,6 (IMW, TAM Cable)
	@thu BIT,			-- required for sales models 5,6 (IMW, TAM Cable)
	@fri BIT,			-- required for sales models 5,6 (IMW, TAM Cable)
	@sat BIT,			-- required for sales models 5,6 (IMW, TAM Cable)
	@sun BIT			-- required for sales models 5,6 (IMW, TAM Cable)
)
RETURNS MONEY
AS
BEGIN
	----------------
	-- FOR DEBUGGING
	----------------
	--DECLARE 
	--	@network_rate_card_book_id INT,
	--	@rate_card_type_id TINYINT,
	--	@network_id INT,
	--	@rate_card_daypart_id INT, -- not required for sales models 5,6 (IMW, TAM Cable)
	--	@effective_date DATE,
	--	@spot_length_id INT, -- not required for sales models 5,6 (IMW, TAM Cable)
	--	@hh_delivery FLOAT, -- NOT IN (000)!, required for sales models 5,6 (IMW, TAM Cable)
	--	-- reprsents ratings daypart
	--	@start_time INT,	-- required for sales models 5,6 (IMW, TAM Cable)
	--	@end_time INT,		-- required for sales models 5,6 (IMW, TAM Cable)
	--	@mon BIT,			-- required for sales models 5,6 (IMW, TAM Cable)
	--	@tue BIT,			-- required for sales models 5,6 (IMW, TAM Cable)
	--	@wed BIT,			-- required for sales models 5,6 (IMW, TAM Cable)
	--	@thu BIT,			-- required for sales models 5,6 (IMW, TAM Cable)
	--	@fri BIT,			-- required for sales models 5,6 (IMW, TAM Cable)
	--	@sat BIT,			-- required for sales models 5,6 (IMW, TAM Cable)
	--	@sun BIT			-- required for sales models 5,6 (IMW, TAM Cable)
	
	--SET @network_rate_card_book_id = 410
	--SET @rate_card_type_id = 1
	--SET @network_id = 1
	--SET @rate_card_daypart_id = 1
	--SET @effective_date = '3/7/2014'
	--SET @spot_length_id = 1
	--SET @hh_delivery = 410323
	--SET @start_time = 21600
	--SET @end_time = 86399
	--SET @mon = 1
	--SET @tue = 1
	--SET @wed = 1
	--SET @thu = 1
	--SET @fri = 1
	--SET @sat = 1
	--SET @sun = 1
		
	DECLARE @return MONEY;
	DECLARE @sales_model_id INT;
	
	SELECT @sales_model_id = nrcb.sales_model_id FROM dbo.network_rate_card_books nrcb (NOLOCK) WHERE nrcb.id=@network_rate_card_book_id;
	
	IF @sales_model_id  = 5 OR @sales_model_id = 6
	BEGIN
		DECLARE @total_hours FLOAT;
		SET @total_hours = dbo.GetTotalHoursFromTimes(@start_time, @end_time);
		
		-- case represents ratio between rate card delivery and @hh_delivery
		SELECT
			@return = nrcr.rate * CASE WHEN nrcd.hh_delivery > 0 THEN @hh_delivery / nrcd.hh_delivery ELSE 1.0 END 
		FROM
			dbo.network_rate_cards nrc (NOLOCK)
			JOIN dbo.network_rate_card_details nrcd (NOLOCK) ON nrcd.network_rate_card_id=nrc.id
				AND nrcd.network_id=@network_id
			JOIN dbo.network_rate_card_rates nrcr (NOLOCK) ON nrcr.network_rate_card_detail_id=nrcd.id
				AND nrcr.spot_length_id=1
		WHERE
			nrc.network_rate_card_book_id=@network_rate_card_book_id
			AND nrc.rate_card_type_id=@rate_card_type_id
			AND nrc.daypart_id=1;
			
		-- apply daypart weighting for daypart(s) could be more than one
		SELECT
			-- commented out code below just for informational purposes about the equation
			--   @return / da.total_hours 'rate_per_hour',
			--   (((@return / da.total_hours) * weighting_factor) - (@return / da.total_hours)) 'increase_per_hour',
			--   (dbo.GetIntersectingDaypartHours(da.start_time,da.end_time,@start_time,@end_time) * dbo.GetIntersectingDaypartDays(@mon,@tue,@wed,@thu,@fri,@sat,@sun, da.mon,da.tue,da.wed,da.thu,da.fri,da.sat,da.sun) 'intersecting_hours',
			@return = @return + SUM(
				CASE
					-- WHEN percentage of overlapping hours is greater than 50%
					WHEN @total_hours > 0.0 AND CAST(dbo.GetIntersectingDaypartHours(da.start_time,da.end_time,@start_time,@end_time) AS FLOAT) / @total_hours >= 0.66 THEN 
						ISNULL((dbo.GetIntersectingDaypartHours(da.start_time,da.end_time,@start_time,@end_time) * dbo.GetIntersectingDaypartDays(@mon,@tue,@wed,@thu,@fri,@sat,@sun, da.mon,da.tue,da.wed,da.thu,da.fri,da.sat,da.sun)) * (((@return / da.total_hours) * nrcda.weight) - (@return / da.total_hours)), 0)
					ELSE
						0
				END)
		FROM
			dbo.network_rate_card_daypart_adjustments nrcda (NOLOCK)
			JOIN vw_ccc_daypart da ON da.id=nrcda.daypart_id
		WHERE
			nrcda.sales_model_id=@sales_model_id
			AND @effective_date BETWEEN nrcda.start_date AND nrcda.end_date
			AND nrcda.network_id=@network_id;
	END
	ELSE
	BEGIN
		SELECT
			@return = nrcr.rate
		FROM
			dbo.network_rate_cards nrc (NOLOCK)
			JOIN dbo.network_rate_card_details nrcd (NOLOCK) ON nrcd.network_rate_card_id=nrc.id
				AND nrcd.network_id=@network_id
			JOIN dbo.network_rate_card_rates nrcr (NOLOCK) ON nrcr.network_rate_card_detail_id=nrcd.id
				AND nrcr.spot_length_id=@spot_length_id
		WHERE
			nrc.network_rate_card_book_id=@network_rate_card_book_id
			AND nrc.rate_card_type_id=@rate_card_type_id
			AND nrc.daypart_id=@rate_card_daypart_id;
	END
	
	RETURN @return;
END
