-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/28/2010
-- Change date:	12/102/2014
-- Description:	Get's the forecasted US Universe based on the passed @active_weeks parameter.
-- Sample:		EXEC usp_ARS_GetUSUniverseByAudience 61,6,393,1,'10/27/2014-11/09/2014,12/1/2014-12/7/2014,12/8/2014-12/14/2014,12/22/2014-12/28/2014'
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARS_GetUSUniverseByAudience]
	@audience_id INT,
	@sales_model_id INT,
	@base_media_month_id INT,
	@rating_source_id TINYINT,
	@active_weeks VARCHAR(MAX) -- comma separated list of start and end dates (which fall in media weeks) (example: '10/27/2014-11/09/2014,12/1/2014-12/7/2014,12/8/2014-12/14/2014,12/22/2014-12/28/2014')
AS
BEGIN
	-- DEBUG
	--DECLARE
	--	@audience_id INT,
	--	@sales_model_id INT,
	--	@base_media_month_id INT,
	--	@rating_source_id TINYINT,
	--	@active_weeks VARCHAR(MAX) -- comma separated list of start and end dates (which fall in media weeks) (example: '12/1/2014-12/7/2014,12/8/2014-12/14/2014,12/22/2014-12/28/2014'

	--SET @audience_id = 61
	--SET @sales_model_id = 6
	--SET @base_media_month_id = (SELECT id FROM media_months WHERE media_month='0614')
	--SET @rating_source_id = 1
	--SET @active_weeks = '10/27/2014-11/09/2014,12/1/2014-12/7/2014,12/8/2014-12/14/2014,12/22/2014-12/28/2014'
	
	DECLARE @default_rating_source_id TINYINT
	SELECT @default_rating_source_id = rs.default_rating_source_id FROM rating_sources rs (NOLOCK) WHERE rs.id=@rating_source_id
	
	CREATE TABLE #active_weeks_in_month (media_month_id INT, num_active_weeks INT)
	INSERT INTO #active_weeks_in_month
		SELECT
			mw.media_month_id,
			COUNT(1) 'num_active_weeks'
		FROM
			dbo.SplitFlightRanges(@active_weeks) fr
			JOIN media_weeks mw (NOLOCK) ON mw.id=fr.media_week_id
		GROUP BY
			mw.media_month_id;
		
	DECLARE @total_weeks INT
	SELECT
		@total_weeks = SUM(awim.num_active_weeks)
	FROM
		#active_weeks_in_month awim
		
	IF @total_weeks = 0
	BEGIN
		SELECT 0
		RETURN;
	END
				
	SELECT
		SUM(tmp.universe) / @total_weeks
	FROM (
		SELECT
			u.forecast_media_month_id,
			SUM(u.universe) * awim.num_active_weeks 'universe' -- this sums demographic components if there are more than one, then weights it by the number of active weeks in the month
		FROM
			universes u (NOLOCK)
			JOIN rating_source_rating_categories rsrc (NOLOCK) ON rsrc.rating_category_id=u.rating_category_id
				AND rsrc.rating_source_id=CASE WHEN @default_rating_source_id IS NOT NULL THEN @default_rating_source_id ELSE @rating_source_id END
			JOIN rating_categories rc (NOLOCK) ON rc.id=rsrc.rating_category_id
			JOIN audience_audiences aa (NOLOCK) ON u.audience_id = aa.rating_audience_id
				AND aa.rating_category_group_id=rc.rating_category_group_id
			JOIN #active_weeks_in_month awim ON awim.media_month_id=u.forecast_media_month_id
		WHERE 
			u.base_media_month_id = @base_media_month_id
			AND aa.custom_audience_id = @audience_id
			AND u.nielsen_network_id= 
				CASE @sales_model_id 
					WHEN 2 THEN 347	-- TotUSH
					WHEN 3 THEN 347	-- TotUSH
					ELSE 336		-- TotUS
				END
		GROUP BY
			u.forecast_media_month_id,
			awim.num_active_weeks
	) tmp
	
	DROP TABLE #active_weeks_in_month;
END
