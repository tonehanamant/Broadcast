
CREATE PROCEDURE [dbo].[GetCustomRatings]
(           
             @requests CustomRatingsRequest READONLY,
             @base_media_month_id AS INT,
             @start_date AS DATETIME,
             @end_date AS DATETIME,
             @biases AS INT,
             @rating_source_id TINYINT,
             @business_id INT, -- OPTIONAL, used for MSO specific rotational bias, only used when @biases is turned on for @bias_rotational
			 @media_weeks MediaWeeksInput READONLY,
			 @distinct_networks DistinctNetworkInformation READONLY,
			 @rating_category_group_id SMALLINT,
			 @rating_category_ids UniqueIdTable READONLY,
			 @audience_information AudienceInformation READONLY ,
			 @rating_component_dayparts RatingComponentDayparts READONLY,
			 @hours_of_week HoursOfWeek READONLY
)
AS
BEGIN
	-- get number of active weeks per media month
	DECLARE @active_weeks_by_month TABLE (
		media_month_id INT NOT NULL, 
		num_active_weeks_in_media_month FLOAT NOT NULL, 
		ratio_of_active_weeks FLOAT NOT NULL,
		PRIMARY KEY (media_month_id)
	)
	INSERT INTO @active_weeks_by_month
		SELECT
			mw.media_month_id,
			COUNT(1),
			COUNT(1) / (SELECT COUNT(1) FROM @media_weeks mw WHERE mw.selected=1)
		FROM
			@media_weeks mw
		WHERE
			mw.selected=1
		GROUP BY
			mw.media_month_id
	-- get forecast months based on start date and end date
	DECLARE @forecast_media_months TABLE (media_month_id INT PRIMARY KEY) 

	INSERT INTO @forecast_media_months
		SELECT media_month_id FROM @active_weeks_by_month

	DECLARE @total_expected_forecast_media_months INT
	SELECT @total_expected_forecast_media_months = COUNT(1) FROM @forecast_media_months
					
	DECLARE
	-- BITMASK - DO NOT CHANGE!!!
	@bias_rotational INT = 1

	-- ratings_1 aggregrates by component demographic while applying network substitutions (@factor_delivery and @factor_universe)
	--   and conditionally applies expert and/or C3 bias (with @factor_all)
	DECLARE @ratings_results TABLE (request_id INT, 
									forecast_media_month_id INT, 
									component_daypart_id INT, 
									rating FLOAT, 
									us_universe FLOAT,
									PRIMARY KEY (request_id, forecast_media_month_id, component_daypart_id));

	INSERT INTO @ratings_results
		SELECT
			req.id,
			r.forecast_media_month_id,
			dp.component_daypart_id,
			(SUM(r.audience_usage) * dn.delivery_factor) / (SUM(u.universe) * dn.universe_factor) * dn.bias 'rating',
			SUM(u.universe * dn.universe_factor) 'us_universe'
		FROM
			@requests req
			JOIN @distinct_networks dn on dn.network_id = req.network_id
			JOIN dbo.ratings r (NOLOCK) ON r.base_media_month_id = @base_media_month_id
				AND r.nielsen_network_id = dn.nielsen_network_id_delivery
			JOIN @forecast_media_months fmm ON fmm.media_month_id=r.forecast_media_month_id
			JOIN @rating_category_ids rc ON rc.id=r.rating_category_id
			JOIN @audience_information aa ON r.audience_id = aa.rating_audience_id
				AND aa.custom_audience_id = req.audience_id
			JOIN @rating_component_dayparts dp ON dp.request_id=req.id
				AND dp.component_daypart_id = r.daypart_id
			JOIN dbo.universes u (NOLOCK) ON u.rating_category_id=rc.id
				AND u.base_media_month_id	   = r.base_media_month_id
				AND u.forecast_media_month_id  = r.forecast_media_month_id
				AND u.nielsen_network_id	   = dn.nielsen_network_id_universe
				AND u.audience_id			   = aa.rating_audience_id
		GROUP BY
			req.id,
			dn.delivery_factor,
			dn.universe_factor,
			dn.bias,
			r.forecast_media_month_id,
			dp.component_daypart_id

	-- the ratings part is split into two parts below, if rotational bias was passed as a paramter the first part of the IF runs,
	--    if not, the ELSE runs. It was split to reduce the work the procedure has to do.
	-- if the interaction with the ratings/universes tables needs to change it needs to change in both parts of the IF/ELSE below.
	IF ((@biases & @bias_rotational) = @bias_rotational)
	BEGIN
		-- rotational bias dimension
		DECLARE @rotational_baises TABLE (
			request_id INT NOT NULL,
			forecast_media_month_id INT NOT NULL, 
			component_daypart_id INT NOT NULL,
			subscribers BIGINT,
			PRIMARY KEY (request_id,forecast_media_month_id,component_daypart_id, subscribers) 
		)
		
		IF @business_id IS NULL
		BEGIN
			INSERT INTO @rotational_baises
				SELECT
					r.id,
					fmm.media_month_id,
					dp.component_daypart_id,
					ISNULL(SUM(rbc.subscribers),1.0)
				FROM
					@requests r
					JOIN @distinct_networks dn on dn.network_id = r.network_id
					CROSS APPLY @forecast_media_months fmm
					JOIN @active_weeks_by_month awbm ON awbm.media_month_id=fmm.media_month_id
					JOIN @media_weeks mw ON mw.media_month_id=fmm.media_month_id
						AND mw.selected=1
					JOIN @rating_component_dayparts dp ON dp.request_id=r.id
					JOIN @hours_of_week how  ON how.component_daypart_id = dp.component_daypart_id
					LEFT JOIN dbo.uvw_rotational_bias_coefficients_level_1 rbc (NOLOCK) ON rbc.rule_code=dn.network_rule_type
						AND rbc.base_media_month_id=@base_media_month_id
						AND rbc.forecast_media_month_id=fmm.media_month_id
						AND rbc.week_number=mw.week_number
						AND rbc.network_id=r.network_id
						AND rbc.hour_of_week=how.hour_of_week
				GROUP BY
					r.id,
					fmm.media_month_id,
					dp.component_daypart_id
		END
		ELSE
		BEGIN
			INSERT INTO @rotational_baises
				SELECT
					r.id,
					fmm.media_month_id,
					dp.component_daypart_id,
					ISNULL(SUM(rbc.subscribers),1.0)
				FROM
					@requests r
					JOIN @distinct_networks dn on dn.network_id = r.network_id
					CROSS APPLY @forecast_media_months fmm
					JOIN @active_weeks_by_month awbm ON awbm.media_month_id=fmm.media_month_id
					JOIN @media_weeks mw ON mw.media_month_id=fmm.media_month_id
						AND mw.selected=1
					JOIN @rating_component_dayparts dp on dp.request_id = r.id
					JOIN @hours_of_week how  ON how.component_daypart_id = dp.component_daypart_id
					LEFT JOIN dbo.rotational_bias_coefficients rbc (NOLOCK) ON rbc.rule_code=dn.network_rule_type
						AND rbc.base_media_month_id=@base_media_month_id
						AND rbc.forecast_media_month_id=fmm.media_month_id
						AND rbc.week_number=mw.week_number
						AND rbc.network_id=r.network_id
						AND rbc.hour_of_week=how.hour_of_week
						AND rbc.business_id=@business_id
				GROUP BY
					r.id,
					fmm.media_month_id,
					dp.component_daypart_id
		END
		
		-- actual ratings math with rotational bias, weighting by daypart hours and weighting by active weeks in forecast month
			SELECT
				ratings_1.request_id,
				AVG(ratings_1.us_universe) 'us_universe',
				-- this weights the ratings by rotational bias coefficients, intersecting daypart hours, and active weeks in month
				CASE WHEN SUM(rb.subscribers * cd.intersecting_hours * awbm.num_active_weeks_in_media_month) > 0 
					THEN 
						SUM(ratings_1.rating * rb.subscribers * cd.intersecting_hours * awbm.num_active_weeks_in_media_month) / SUM(rb.subscribers * cd.intersecting_hours * awbm.num_active_weeks_in_media_month) 
					ELSE 
						SUM(ratings_1.rating) 
				END 'rating'
			FROM 
				@ratings_results ratings_1
				-- used to weight by rotational bias coefficients
				JOIN @rotational_baises rb ON rb.request_id=ratings_1.request_id
					AND rb.forecast_media_month_id=ratings_1.forecast_media_month_id
					AND rb.component_daypart_id=ratings_1.component_daypart_id
				-- used to weight by intersecting daypart hours
				JOIN @rating_component_dayparts cd ON cd.component_daypart_id=ratings_1.component_daypart_id 
					AND cd.request_id = ratings_1.request_id
				-- used to weight by active weeks per forecast month
				JOIN @active_weeks_by_month awbm ON awbm.media_month_id=ratings_1.forecast_media_month_id
			GROUP BY
				ratings_1.request_id
	END
	ELSE
	BEGIN
		-- actual ratings math with weighting by daypart hours and weighting by active weeks in forecast month (no rotational bias)
			SELECT
				ratings_1.request_id,
				AVG(ratings_1.us_universe) 'us_universe',
				-- this weights the ratings by rotational bias coefficients, intersecting daypart hours, and active weeks in month
				CASE WHEN SUM(cd.intersecting_hours * awbm.num_active_weeks_in_media_month) > 0 
					THEN 
						SUM(ratings_1.rating * cd.intersecting_hours * awbm.num_active_weeks_in_media_month) / SUM(cd.intersecting_hours * awbm.num_active_weeks_in_media_month) 
					ELSE 
						SUM(ratings_1.rating) 
				END 'rating'
			FROM 
				@ratings_results ratings_1
				-- used to weight by intersecting daypart hours
				JOIN @rating_component_dayparts cd ON cd.component_daypart_id=ratings_1.component_daypart_id AND cd.request_id = ratings_1.request_id
				-- used to weight by active weeks per forecast month
				JOIN @active_weeks_by_month awbm ON awbm.media_month_id=ratings_1.forecast_media_month_id
			GROUP BY
				ratings_1.request_id
	END

	RETURN;	
END