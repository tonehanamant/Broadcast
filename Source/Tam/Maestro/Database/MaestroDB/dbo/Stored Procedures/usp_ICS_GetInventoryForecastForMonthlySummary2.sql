-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/16/2015
-- Description:	Returns lastest inventory forecast specific to the dimensions of network and media month.
-- =============================================
/*
	DECLARE @media_month_id SMALLINT = 408			
	DECLARE @inventory_state_filter_code TINYINT = 0
	DECLARE @inventory_flight_selection_code TINYINT = 0
	EXEC usp_ICS_GetInventoryForecastForMonthlySummary @media_month_id, @inventory_state_filter_code, @inventory_flight_selection_code, 0
*/
CREATE PROCEDURE [dbo].[usp_ICS_GetInventoryForecastForMonthlySummary2]
	@media_month_id INT,
	@inventory_state_filter_code TINYINT, -- 0 = Planned & Ordered, 1 = Ordered Only, 2 = Planned Only
	@inventory_flight_selection_code TINYINT, -- 0 = Month Only, 1 = Full Flight
	@use_full_cpm_range BIT
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
	
	DECLARE @proposal_status_ids TABLE (proposal_status_id INT);
	IF @inventory_state_filter_code = 0
		INSERT INTO @proposal_status_ids SELECT id FROM proposal_statuses;
	ELSE IF @inventory_state_filter_code = 1
		INSERT INTO @proposal_status_ids SELECT 4;
	ELSE IF @inventory_state_filter_code = 2
		INSERT INTO @proposal_status_ids SELECT id FROM proposal_statuses WHERE id<>4;
	
	DECLARE @proposals_on_avail_planner TABLE (proposal_id INT NOT NULL, inventory_state_code TINYINT NOT NULL, PRIMARY KEY (proposal_id))
	INSERT INTO @proposals_on_avail_planner
		SELECT
			p.id,
			CASE p.proposal_status_id WHEN 4 THEN 2 ELSE 1 END 'inventory_state_code'
		FROM
			proposals p
			JOIN proposal_details pd ON pd.proposal_id=p.id
			JOIN proposal_detail_worksheets pdw ON pdw.proposal_detail_id=pd.id
				AND pdw.units>0
			JOIN media_weeks mw ON mw.id=pdw.media_week_id
				AND mw.media_month_id=@media_month_id
			JOIN @proposal_status_ids ps ON ps.proposal_status_id=p.proposal_status_id
		WHERE
			p.include_on_availability_planner=1
			AND p.total_gross_cost>0 -- filter out zero cost plans
		GROUP BY
			p.id,
			CASE p.proposal_status_id WHEN 4 THEN 2 ELSE 1 END
	
	DECLARE @relevant_media_month_ids TABLE (media_month_id SMALLINT);
	
	IF @inventory_flight_selection_code = 1
	BEGIN
		INSERT INTO @relevant_media_month_ids
			SELECT
				mw.media_month_id
			FROM
				@proposals_on_avail_planner p
				JOIN proposal_flights pf ON pf.proposal_id=p.proposal_id
					AND pf.selected=1
				JOIN media_weeks mw ON pf.start_date BETWEEN mw.start_date AND mw.end_date
			GROUP BY
				mw.media_month_id;
	END
	ELSE
	BEGIN
		INSERT INTO @relevant_media_month_ids
			SELECT @media_month_id
	END
	
	DECLARE @start_date DATETIME;
	SELECT @start_date = MIN(mm.start_date) FROM @relevant_media_month_ids rmm JOIN media_months mm ON mm.id=rmm.media_month_id;
	
	DECLARE @latest_base_media_month_id SMALLINT;
	SELECT @latest_base_media_month_id = dbo.udf_GetLatestPossibleInventoryForecastBaseMediaMonthId(@start_date);

	IF @use_full_cpm_range = 1
	BEGIN
		SELECT
			inv.forecast_media_month_id 'ForecastMediaMonthId',
			inv.forecast_media_week_id 'ForecastMediaWeekId',
			inv.network_id 'NetworkId',
			inv.component_daypart_id 'ComponentDaypartId',
			inv.hh_eq_cpm_start 'HhEqCpmStart',
			inv.hh_eq_cpm_end 'HhEqCpmEnd',
			SUM(inv.subscribers) 'Subscribers'
		FROM
			dbo.inventory_forecasts inv
			JOIN @relevant_media_month_ids rmmi ON rmmi.media_month_id=inv.forecast_media_month_id
		WHERE
			inv.base_media_month_id=@latest_base_media_month_id
		GROUP BY
			inv.forecast_media_month_id,
			inv.forecast_media_week_id,
			inv.network_id,
			inv.component_daypart_id,
			inv.hh_eq_cpm_start,
			inv.hh_eq_cpm_end
	END
	ELSE
	BEGIN
		SELECT
			inv.forecast_media_month_id 'ForecastMediaMonthId',
			inv.forecast_media_week_id 'ForecastMediaWeekId',
			inv.network_id 'NetworkId',
			inv.component_daypart_id 'ComponentDaypartId',
			inv.hh_eq_cpm_start 'HhEqCpmStart',
			inv.hh_eq_cpm_end 'HhEqCpmEnd',
			SUM(inv.subscribers) 'Subscribers'
		FROM
			dbo.inventory_forecasts_reduced inv
			JOIN @relevant_media_month_ids rmmi ON rmmi.media_month_id=inv.forecast_media_month_id
		WHERE
			inv.base_media_month_id=@latest_base_media_month_id
		GROUP BY
			inv.forecast_media_month_id,
			inv.forecast_media_week_id,
			inv.network_id,
			inv.component_daypart_id,
			inv.hh_eq_cpm_start,
			inv.hh_eq_cpm_end	
	END
END