-- =============================================
-- Author:           Stephen DeFusco
-- Create date: 11/22/2011
-- Modified:  07/17/2015 - updated for long term ratings forecast update, rotational bias forecast update, and refactored so that all logic is in this procedure for weighting.
--                         08/31/2015 - updated to include optional @business_id parameter to perform business specific rotational bias.
--                         10/16/2015 - updated the way in which the @forecasted_media_months table is populated to account for unexpected inputs.
--						   12/06/2016 - updated to enfoce rotational bias ceiling
-- Description:      This is the single reference point for querying forecasting ratings.
-- =============================================
/*
       DECLARE @biases INT = 3 -- BITMASK: bias_rotational=1, bias_expert=2     
       DECLARE @hiatus_weeks FlightTable
       INSERT INTO @hiatus_weeks SELECT '2015-10-12','2015-10-18'
       
       -- TEST
       SELECT 
              x.us_universe,x.rating 
       FROM 
              dbo.udf_GetCustomRatings(1,33,399,'2015-09-28','2015-11-29',36000,86399,1,1,1,1,1,1,1,0,1,@hiatus_weeks,NULL) x
       SELECT 
              x.us_universe,x.rating 'with bias_rotational' 
       FROM 
              dbo.udf_GetCustomRatings(1,33,399,'2015-09-28','2015-11-29',36000,86399,1,1,1,1,1,1,1,1,1,@hiatus_weeks,NULL) x
       SELECT 
              x.us_universe,x.rating 'with bias_expert' 
       FROM 
              dbo.udf_GetCustomRatings(1,33,399,'2015-09-28','2015-11-29',36000,86399,1,1,1,1,1,1,1,2,1,@hiatus_weeks,NULL) x
       SELECT 
              x.us_universe,x.rating 'with bias_rotational and bias_expert' 
       FROM 
              dbo.udf_GetCustomRatings(1,33,399,'2015-09-28','2015-11-29',36000,86399,1,1,1,1,1,1,1,@biases,1,@hiatus_weeks,NULL) x
              
       
       -- TEST WITH COMCAST SPECIFIC ROTATIONAL BIAS
       SELECT 
              x.us_universe,x.rating 
       FROM 
              dbo.udf_GetCustomRatings(1,33,399,'2015-09-28','2015-11-29',36000,86399,1,1,1,1,1,1,1,0,1,@hiatus_weeks,8) x
       SELECT 
              x.us_universe,x.rating 'with bias_rotational (comcast)' 
       FROM 
              dbo.udf_GetCustomRatings(1,33,399,'2015-09-28','2015-11-29',36000,86399,1,1,1,1,1,1,1,1,1,@hiatus_weeks,8) x
       SELECT 
              x.us_universe,x.rating 'with bias_expert' 
       FROM 
              dbo.udf_GetCustomRatings(1,33,399,'2015-09-28','2015-11-29',36000,86399,1,1,1,1,1,1,1,2,1,@hiatus_weeks,8) x
       SELECT 
              x.us_universe,x.rating 'with bias_rotational (comcast) and bias_expert' 
       FROM 
              dbo.udf_GetCustomRatings(1,33,399,'2015-09-28','2015-11-29',36000,86399,1,1,1,1,1,1,1,@biases,1,@hiatus_weeks,8) x
*/
CREATE FUNCTION [dbo].[udf_GetCustomRatings]
(      
       @network_id AS INT,
       @audience_id AS INT,
       @base_media_month_id AS INT,
       @start_date AS DATETIME,
       @end_date AS DATETIME,
       @start_time AS INT,
       @end_time AS INT,
       @mon AS BIT,
       @tue AS BIT,
       @wed AS BIT,
       @thu AS BIT,
       @fri AS BIT,
       @sat AS BIT,
       @sun AS BIT,
       @biases AS INT,
       @rating_source_id TINYINT,
       @hiatus_weeks FlightTable READONLY,
       @business_id INT -- OPTIONAL, used for MSO specific rotational bias, only used when @biases is turned on for @bias_rotational
)
RETURNS @return TABLE 
(
       us_universe FLOAT,
       rating FLOAT
)
AS
BEGIN
       DECLARE
              @nielsen_network_id INT,
              @nielsen_network_id_delivery INT,
              @nielsen_network_id_universe INT,
              @factor_delivery FLOAT = 1,
              @factor_universe FLOAT = 1,
              @factor_all FLOAT = 1,
              @factor_expert_bias FLOAT = 1,
              @factor_c3_bias FLOAT = 1,
              @total_daypart_hours INT,
              @rating_category_group_id TINYINT,
              @total_active_media_weeks FLOAT,
              @total_expected_forecast_media_months INT,
              @network_rule_type TINYINT = NULL,
              -- BITMASK - DO NOT CHANGE!!!
              @bias_rotational INT = 1,
              @bias_expert INT = 2
              -- BITMASK - DO NOT CHANGE!!!

       -- correlate start date/end date and hiatus weeks with our media weeks and media months
       DECLARE @media_weeks AS TABLE (
              media_month_id INT NOT NULL, 
              media_week_id INT NOT NULL, 
              week_number TINYINT NOT NULL, 
              start_date DATE NOT NULL, 
              end_date DATE NOT NULL, 
              selected BIT NOT NULL,
              UNIQUE CLUSTERED (media_month_id, media_week_id) 
       )
       INSERT INTO @media_weeks
              SELECT
                     mw.media_month_id,
                     mw.id,
                     mw.week_number,
                     mw.start_date,
                     mw.end_date,
                     CASE WHEN hw.start_date IS NULL THEN 1 ELSE 0 END 'selected'
              FROM
                     dbo.media_weeks mw (NOLOCK)
                     LEFT JOIN @hiatus_weeks hw ON hw.start_date BETWEEN mw.start_date AND mw.end_date
              WHERE
                     (mw.start_date <= @end_date AND mw.end_date >= @start_date)
              ORDER BY
                     mw.start_date

       -- calculate total number of active weeks in flight
       SELECT @total_active_media_weeks = COUNT(1) FROM @media_weeks mw WHERE mw.selected=1

       -- get number of active weeks per media month
       DECLARE @active_weeks_by_month TABLE (
              media_month_id INT NOT NULL PRIMARY KEY, 
              num_active_weeks_in_media_month FLOAT NOT NULL, 
              ratio_of_active_weeks FLOAT NOT NULL,
              UNIQUE CLUSTERED (media_month_id) 
       )
       INSERT INTO @active_weeks_by_month
              SELECT
                     mw.media_month_id,
                     COUNT(1),
                     COUNT(1) / @total_active_media_weeks
              FROM
                     @media_weeks mw
              WHERE
                     mw.selected=1
              GROUP BY
                     mw.media_month_id
       
       -- get forecast months based on start date and end date
       DECLARE       @forecast_media_months TABLE (
              media_month_id INT NOT NULL PRIMARY KEY,
              UNIQUE CLUSTERED (media_month_id) 
       ) 
       INSERT INTO @forecast_media_months
              SELECT media_month_id FROM @active_weeks_by_month
              
       SELECT @total_expected_forecast_media_months = COUNT(1) FROM @forecast_media_months

       -- get the component dayparts based on the custom daypart
       DECLARE @rating_component_dayparts TABLE (
              component_daypart_id INT NOT NULL PRIMARY KEY,
              start_time INT NOT NULL,
              end_time INT NOT NULL,
              mon INT NOT NULL,
              tue INT NOT NULL,
              wed INT NOT NULL,
              thu INT NOT NULL,
              fri INT NOT NULL,
              sat INT NOT NULL,
              sun INT NOT NULL,
              intersecting_hours SMALLINT,
              weekends BIT,
              weekdays BIT,
              UNIQUE CLUSTERED (component_daypart_id) 
       )
       INSERT INTO @rating_component_dayparts
              SELECT 
                     id,start_time,end_time,mon,tue,wed,thu,fri,sat,sun,
                     -- intersecting hours * intersecting days
                     dbo.GetIntersectingDaypartHours(cd.start_time,cd.end_time,@start_time,@end_time) * ((@mon & cd.mon) + (@tue & cd.tue) + (@wed & cd.wed) + (@thu & cd.thu) + (@fri & cd.fri) + (@sat & cd.sat) + (@sun & cd.sun)),
                     (cd.sat & cd.sun) 'weekends',
                     (cd.mon & cd.tue & cd.wed & cd.thu & cd.fri) 'weekydays'
              FROM 
                     dbo.GetDaypartComponents(@start_time,@end_time,@mon,@tue,@wed,@thu,@fri,@sat,@sun) cd
                     
       -- lookup the network rule type (0=Static, 1=Seasonal, 2=Hybrid), Default Hybrid when none is defined for a network.
       SELECT 
              @network_rule_type = CASE nm.map_value WHEN 'Static' THEN 0 WHEN 'Seasonal' THEN 1 ELSE 2 END -- hybrid default
       FROM
              dbo.network_maps nm (NOLOCK)
       WHERE
              nm.map_set='RotBiasRules'
              AND nm.network_id=@network_id;
              
       IF @network_rule_type IS NULL
              SET @network_rule_type = 2 -- hybrid default

       -- get rating category group id of the ratings source parameter (used and required to filter audience_audiences table)
       SELECT @rating_category_group_id = dbo.GetRatingCategoryGroupIdOfRatingsSource(@rating_source_id)

       -- calculate the total hours of the daypart parameter
       SET @total_daypart_hours = 
              cast(@mon as int) + cast(@tue as int) + cast(@wed as int) + cast(@thu as int) + cast(@fri as int) + cast(@sat as int) + cast(@sun as int)
       IF @start_time > @end_time
              SET @total_daypart_hours = @total_daypart_hours * ABS(ROUND(((86400 - @start_time) + (@end_time+1)) / 3600,0))
       ELSE
              SET @total_daypart_hours = @total_daypart_hours * ABS(ROUND((@end_time+1-@start_time) / 3600,0))

       -- lookup nielsen_network_id
       SELECT 
              @nielsen_network_id = nn.id
       FROM 
              dbo.nielsen_networks nn (NOLOCK)
       WHERE  
              nn.nielsen_id IN (SELECT map_value FROM dbo.network_maps (NOLOCK) WHERE map_set='Nielsen' AND network_id=@network_id)
              
       -- look for "delivery" network substitution and delivery factor
       SELECT 
              @nielsen_network_id_delivery = nn.id,
              @factor_delivery = ns.weight
       FROM 
              dbo.uvw_network_substitutions ns (NOLOCK)
              JOIN dbo.network_maps               nm (NOLOCK) ON (nm.map_set='Nielsen' AND nm.network_id=ns.substitute_network_id)
              JOIN dbo.nielsen_networks    nn (NOLOCK) ON (nm.map_value=nn.nielsen_id) 
       WHERE  
              ns.network_id = @network_id
              AND (ns.start_date<=@start_date AND (ns.end_date>=@start_date OR ns.end_date IS NULL))
              AND ns.substitution_category_id=1 -- delivery
              AND ns.rating_category_group_id = @rating_category_group_id
              
       -- look for "universe" network substitution and universe factor
       SELECT 
              @nielsen_network_id_universe = nn.id,
              @factor_universe = ns.weight
       FROM 
              dbo.uvw_network_substitutions ns (NOLOCK)
              JOIN dbo.network_maps             nm   (NOLOCK) ON (nm.map_set='Nielsen' AND nm.network_id=ns.substitute_network_id)
              JOIN dbo.nielsen_networks  nn   (NOLOCK) ON (nm.map_value=nn.nielsen_id) 
       WHERE  
              ns.network_id = @network_id 
              AND (ns.start_date<=@start_date AND (ns.end_date>=@start_date OR ns.end_date IS NULL))
              AND ns.substitution_category_id=2 -- universe
              AND ns.rating_category_group_id = @rating_category_group_id

       -- required NULL check, don't remove this
       IF @nielsen_network_id_delivery IS NULL
              SET @nielsen_network_id_delivery = @nielsen_network_id
       IF @nielsen_network_id_universe IS NULL
              SET @nielsen_network_id_universe = @nielsen_network_id

       IF @rating_category_group_id = 1 -- Nielsen MIT
       BEGIN
              -- expert bias
              IF ((@biases & @bias_expert) = @bias_expert)
              BEGIN
                     SELECT 
                           @factor_expert_bias = ISNULL(bias, 1)
                     FROM 
                           dbo.expert_biases (NOLOCK)
                     WHERE 
                           media_month_id                    = @base_media_month_id 
                           AND nielsen_network_id     = @nielsen_network_id 
                           AND audience_id                   = 31 -- HH

                     IF @factor_expert_bias IS NOT NULL
                           SET @factor_all = @factor_all * @factor_expert_bias
              END

              -- c3 bias (if ratings source is legacy c3 ratings source (id=3))
              IF (@rating_source_id = 3)
              BEGIN
                     SET @rating_source_id = 1; -- set to MIT Live
                     SELECT 
                           @factor_c3_bias = AVG(ISNULL(bias, 1))
                     FROM 
                           dbo.c3_biases (NOLOCK)
                     WHERE 
                           media_month_id                    = @base_media_month_id 
                           AND nielsen_network_id     = @nielsen_network_id 
                           AND audience_id                   = 31 -- HH
                           AND daypart_id                    IN (SELECT id FROM @rating_component_dayparts)

                     IF @factor_c3_bias IS NOT NULL
                           SET @factor_all = @factor_all * @factor_c3_bias
              END
       END
       
       DECLARE @pre_output TABLE (
              us_universe FLOAT,
              rb_rating FLOAT,
              rating FLOAT,
              num_forecast_months INT
       )
       
       -- ratings_1 aggregrates by component demographic while applying network substitutions (@factor_delivery and @factor_universe)
       --   and conditionally applies expert and/or C3 bias (with @factor_all)
       DECLARE @ratings_results TABLE (forecast_media_month_id INT, component_daypart_id INT, rating FLOAT, us_universe FLOAT);
       INSERT INTO @ratings_results
              SELECT
                     r.forecast_media_month_id,
                     dp.component_daypart_id,
                     (SUM(r.audience_usage) * @factor_delivery) / (SUM(u.universe) * @factor_universe) * @factor_all 'rating',
                     SUM(u.universe * @factor_universe) 'us_universe'
              FROM
                     ratings r (NOLOCK)
                     JOIN @forecast_media_months fmm ON fmm.media_month_id=r.forecast_media_month_id
                     JOIN rating_source_rating_categories rsrc (NOLOCK) ON rsrc.rating_category_id=r.rating_category_id
                           AND rsrc.rating_source_id = @rating_source_id
                     JOIN audience_audiences aa (NOLOCK) ON r.audience_id = aa.rating_audience_id
                           AND aa.custom_audience_id = @audience_id
                           AND aa.rating_category_group_id = @rating_category_group_id
                     JOIN @rating_component_dayparts dp ON dp.component_daypart_id = r.daypart_id
                     JOIN universes u (NOLOCK) ON u.rating_category_id=rsrc.rating_category_id
                           AND u.base_media_month_id         = r.base_media_month_id
                           AND u.forecast_media_month_id     = r.forecast_media_month_id
                           AND u.nielsen_network_id          = @nielsen_network_id_universe
                           AND u.audience_id                        = aa.rating_audience_id
              WHERE
                     r.base_media_month_id = @base_media_month_id
                     AND r.nielsen_network_id = @nielsen_network_id_delivery
              GROUP BY
                     r.forecast_media_month_id,
                     dp.component_daypart_id;   
       
       -- check to make sure the daypart requested has rated component dayparts, if not return zeros.
       DECLARE @num_intersecting_dayparts INT;
       SELECT
              @num_intersecting_dayparts = COUNT(1)
       FROM
              @ratings_results rr;

       IF @num_intersecting_dayparts = 0
       BEGIN
              INSERT INTO @return (us_universe,rating) VALUES (0, 0);
              RETURN;
       END
       
       -- the ratings part is split into two parts below, if rotational bias was passed as a paramter the first part of the IF runs,
       --    if not, the ELSE runs. It was split to reduce the work the procedure has to do.
       -- if the interaction with the ratings/universes tables needs to change it needs to change in both parts of the IF/ELSE below.
       IF ((@biases & @bias_rotational) = @bias_rotational)
       BEGIN
              -- rotational bias dimension
              DECLARE @rotational_baises TABLE (
                     forecast_media_month_id INT NOT NULL, 
                     component_daypart_id INT NOT NULL,
                     subscribers BIGINT,
                     UNIQUE CLUSTERED (forecast_media_month_id,component_daypart_id) 
              )
              IF @business_id IS NULL
              BEGIN
                     INSERT INTO @rotational_baises
                           SELECT
                                  fmm.media_month_id,
                                  dp.component_daypart_id,
                                  ISNULL(SUM(rbc.subscribers),1.0)
                           FROM
                                  @forecast_media_months fmm
                                  JOIN @active_weeks_by_month awbm ON awbm.media_month_id=fmm.media_month_id
                                  JOIN @media_weeks mw ON mw.media_month_id=fmm.media_month_id
                                         AND mw.selected=1
                                  CROSS APPLY @rating_component_dayparts dp
                                  JOIN dbo.hours_of_week how (NOLOCK) ON ((dp.weekdays = (how.mon | how.tue | how.wed | how.thu | how.fri)) OR (dp.weekends = (how.sat | how.sun)))
                                         AND (dp.start_time <= how.end_time AND dp.end_time >= how.start_time)
                                  LEFT JOIN dbo.uvw_rotational_bias_coefficients_level_1 rbc (NOLOCK) ON rbc.rule_code=@network_rule_type
                                         AND rbc.base_media_month_id=@base_media_month_id
                                         AND rbc.forecast_media_month_id=fmm.media_month_id
                                         AND rbc.week_number=mw.week_number
                                         AND rbc.network_id=@network_id
                                         AND rbc.hour_of_week=how.hour_of_week
                           GROUP BY
                                  fmm.media_month_id,
                                  dp.component_daypart_id
                           ORDER BY
                                  fmm.media_month_id,
                                  dp.component_daypart_id
              END
              ELSE
              BEGIN
                     INSERT INTO @rotational_baises
                           SELECT
                                  fmm.media_month_id,
                                  dp.component_daypart_id,
                                  ISNULL(SUM(rbc.subscribers),1.0)
                           FROM
                                  @forecast_media_months fmm
                                  JOIN @active_weeks_by_month awbm ON awbm.media_month_id=fmm.media_month_id
                                  JOIN @media_weeks mw ON mw.media_month_id=fmm.media_month_id
                                         AND mw.selected=1
                                  CROSS APPLY @rating_component_dayparts dp
                                  JOIN dbo.hours_of_week how (NOLOCK) ON ((dp.weekdays = (how.mon | how.tue | how.wed | how.thu | how.fri)) OR (dp.weekends = (how.sat | how.sun)))
                                         AND (dp.start_time <= how.end_time AND dp.end_time >= how.start_time)
                                  LEFT JOIN dbo.rotational_bias_coefficients rbc (NOLOCK) ON rbc.rule_code=@network_rule_type
                                         AND rbc.base_media_month_id=@base_media_month_id
                                         AND rbc.forecast_media_month_id=fmm.media_month_id
                                         AND rbc.week_number=mw.week_number
                                         AND rbc.network_id=@network_id
                                         AND rbc.hour_of_week=how.hour_of_week
                                         AND rbc.business_id=@business_id
                           GROUP BY
                                  fmm.media_month_id,
                                  dp.component_daypart_id
                           ORDER BY
                                  fmm.media_month_id,
                                  dp.component_daypart_id
              END
                           
              -- actual ratings math with rotational bias, weighting by daypart hours and weighting by active weeks in forecast month
              INSERT INTO @pre_output
                     SELECT
                           AVG(ratings_1.us_universe) 'us_universe',
                           -- this weights the ratings by rotational bias coefficients, intersecting daypart hours, and active weeks in month
                           CASE WHEN SUM(rb.subscribers * cd.intersecting_hours * awbm.num_active_weeks_in_media_month) > 0 
                                  THEN 
                                         SUM(ratings_1.rating * rb.subscribers * cd.intersecting_hours * awbm.num_active_weeks_in_media_month) / SUM(rb.subscribers * cd.intersecting_hours * awbm.num_active_weeks_in_media_month) 
                                  ELSE 
                                         AVG(ratings_1.rating) 
                           END 'rb_rating',
                           CASE WHEN SUM(cd.intersecting_hours * awbm.num_active_weeks_in_media_month) > 0 
                                  THEN 
                                         SUM(ratings_1.rating * cd.intersecting_hours * awbm.num_active_weeks_in_media_month) / SUM(cd.intersecting_hours * awbm.num_active_weeks_in_media_month) 
                                  ELSE 
                                         AVG(ratings_1.rating) 
                           END 'rating',
                           COUNT(DISTINCT ratings_1.forecast_media_month_id) 'num_forecast_months'
                     FROM 
                           @ratings_results ratings_1
                     -- used to weight by rotational bias coefficients
                     JOIN @rotational_baises rb ON rb.forecast_media_month_id=ratings_1.forecast_media_month_id
                           AND rb.component_daypart_id=ratings_1.component_daypart_id
                     -- used to weight by intersecting daypart hours
                     JOIN @rating_component_dayparts cd ON cd.component_daypart_id=ratings_1.component_daypart_id
                     -- used to weight by active weeks per forecast month
                     JOIN @active_weeks_by_month awbm ON awbm.media_month_id=ratings_1.forecast_media_month_id
       END
       ELSE
       BEGIN
              -- actual ratings math with weighting by daypart hours and weighting by active weeks in forecast month (no rotational bias)
              INSERT INTO @pre_output
                     SELECT
                           AVG(ratings_1.us_universe) 'us_universe',
                           NULL 'rb_rating',
                           -- this weights the ratings by rotational bias coefficients, intersecting daypart hours, and active weeks in month
                           CASE WHEN SUM(cd.intersecting_hours * awbm.num_active_weeks_in_media_month) > 0 
                                  THEN 
                                         SUM(ratings_1.rating * cd.intersecting_hours * awbm.num_active_weeks_in_media_month) / SUM(cd.intersecting_hours * awbm.num_active_weeks_in_media_month) 
                                  ELSE 
                                         AVG(ratings_1.rating) 
                           END 'rating',
                           COUNT(DISTINCT ratings_1.forecast_media_month_id) 'num_forecast_months'
                     FROM 
                           @ratings_results ratings_1
                     -- used to weight by intersecting daypart hours
                     JOIN @rating_component_dayparts cd ON cd.component_daypart_id=ratings_1.component_daypart_id
                     -- used to weight by active weeks per forecast month
                     JOIN @active_weeks_by_month awbm ON awbm.media_month_id=ratings_1.forecast_media_month_id
       END
       
       -- check we have data for all expected forecast months
       DECLARE @total_forecast_months_with_data INT;
       SELECT @total_forecast_months_with_data = num_forecast_months FROM @pre_output;
       
       IF @total_expected_forecast_media_months = @total_forecast_months_with_data
       BEGIN
              -- if rotational bias and base month is greater than or equal to 1016
              IF ((@biases & @bias_rotational) = @bias_rotational) 
				  IF @base_media_month_id >=421 -- 1016
                     INSERT INTO @return (us_universe,rating)
                           SELECT us_universe,IIF(rb_rating < rating, rb_rating, rating) FROM @pre_output
				  ELSE
                     INSERT INTO @return (us_universe,rating)
                           SELECT us_universe,rb_rating FROM @pre_output
              ELSE
                     INSERT INTO @return (us_universe,rating)
                           SELECT us_universe,rating FROM @pre_output
       END
                     
       RETURN;
END