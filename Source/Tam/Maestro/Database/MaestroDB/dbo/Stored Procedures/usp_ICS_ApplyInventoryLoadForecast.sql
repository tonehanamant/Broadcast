-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/30/2017
-- Description:	Applies load forecast to reserved inventory requests.
-- =============================================
CREATE PROCEDURE [dbo].[usp_ICS_ApplyInventoryLoadForecast] 
	@media_plan_competition MediaPlanCompetitionTable READONLY
AS
BEGIN
	SET NOCOUNT ON;

    DECLARE @aggregated_media_plan_competitions TABLE (media_month_id SMALLINT NOT NULL, media_week_id INT NOT NULL, network_id INT NOT NULL, component_daypart_id INT NOT NULL, hh_eq_cpm_start MONEY NOT NULL, hh_eq_cpm_end MONEY NOT NULL, subscribers BIGINT NOT NULL, PRIMARY KEY CLUSTERED(media_month_id,media_week_id,network_id,component_daypart_id,hh_eq_cpm_start,hh_eq_cpm_end));
	DECLARE @calculated_load_forecast TABLE (media_month_id SMALLINT NOT NULL, media_week_id INT NOT NULL, network_id INT NOT NULL, component_daypart_id INT NOT NULL, hh_eq_cpm_start MONEY NOT NULL, hh_eq_cpm_end MONEY NOT NULL, subscribers BIGINT NOT NULL, PRIMARY KEY CLUSTERED(media_month_id,media_week_id,network_id,component_daypart_id,hh_eq_cpm_start,hh_eq_cpm_end));

	DECLARE @component_dayparts TABLE (daypart_id INT NOT NULL, week_part BIT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, PRIMARY KEY CLUSTERED(daypart_id));
	INSERT INTO @component_dayparts
		SELECT
			cd.id,
			CASE WHEN (cd.mon | cd.tue | cd.wed | cd.thu | cd.fri) = 1 THEN 0 ELSE 1 END 'week_part',
			cd.start_time,
			cd.end_time
		FROM 
			dbo.GetInventoryComponentDayparts() cd;

	-- get the earliest media month start date
	DECLARE @start_date DATE
	SELECT @start_date = MAX(mm.start_date) FROM @media_plan_competition mc JOIN media_months mm (NOLOCK) ON mm.id=mc.media_month_id;

	-- lookup latest possible inventory forecast / inventory load base month
	DECLARE @latest_base_media_month_id SMALLINT;
	SET @latest_base_media_month_id = dbo.udf_GetLatestPossibleInventoryForecastBaseMediaMonthId(@start_date);
	
	-- create structure to lookup each forecast month / network horizoin
	DECLARE @horizons TABLE (forecast_media_month_id SMALLINT NOT NULL, network_id INT NOT NULL, week_part BIT NOT NULL, horizon INT, PRIMARY KEY CLUSTERED(forecast_media_month_id,network_id,week_part));
	INSERT INTO @horizons
		SELECT
			mc.media_month_id,
			mc.network_id,
			cd.week_part,
			NULL
		FROM
			@media_plan_competition mc
			JOIN @component_dayparts cd ON cd.daypart_id=mc.component_daypart_id
		GROUP BY
			mc.media_month_id,
			mc.network_id,
			cd.week_part;

	-- calculate horizon from base to forecast month
	UPDATE
		@horizons 
	SET 
		horizon = dbo.udf_CalculateNumMonthsBetweenMediaMonths(forecast_media_month_id,@latest_base_media_month_id);

	-- calculate max horizon in the event the calculated horizon exceeds the max
	DECLARE @max_horizons TABLE (network_id INT NOT NULL, week_part BIT NOT NULL, max_horizon INT NOT NULL, PRIMARY KEY CLUSTERED(network_id,week_part));
	INSERT INTO @max_horizons
		SELECT
			ilfs.network_id,
			ilfs.week_part,
			MAX(ilfs.horizon)
		FROM
			dbo.inventory_load_forecast_settings ilfs (NOLOCK)
		WHERE
			ilfs.base_media_month_id=@latest_base_media_month_id
		GROUP BY
			ilfs.network_id,
			ilfs.week_part;

	-- for those horizon's not yet set the max horizon
	UPDATE
		@horizons 
	SET 
		horizon = lh.max_horizon
	FROM
		@horizons h
		JOIN @max_horizons lh ON lh.network_id=h.network_id
			AND lh.week_part=h.week_part
	WHERE
		horizon IS NULL

	-- lookup the alpha values by forecast month, network, and week part
	DECLARE @load_settings_log TABLE (forecast_media_month_id SMALLINT NOT NULL, network_id INT NOT NULL, week_part BIT NOT NULL, alpha FLOAT NOT NULL, PRIMARY KEY CLUSTERED(forecast_media_month_id,network_id,week_part));
	INSERT INTO @load_settings_log
		SELECT
			h.forecast_media_month_id,
			ilfs.network_id,
			ilfs.week_part,
			ilfs.alpha
		FROM
			@horizons h
			JOIN dbo.inventory_load_forecast_settings ilfs (NOLOCK) ON ilfs.network_id=h.network_id
				AND ilfs.horizon=h.horizon --this is either the desired horizon or max_horizon
				AND ilfs.week_part=h.week_part
		WHERE
			ilfs.base_media_month_id=@latest_base_media_month_id;
	-- aggregate out the proposal specific information (proposal_id, unique_proposal_detail_id, proposal_status_id) leaving the important dimensions in place
	INSERT INTO @aggregated_media_plan_competitions
		SELECT
			media_month_id,
			media_week_id,
			network_id,
			component_daypart_id,
			hh_eq_cpm_start,
			hh_eq_cpm_end,
			SUM(CAST(subscribers AS BIGINT))
		FROM
			@media_plan_competition
		GROUP BY
			media_month_id,
			media_week_id,
			network_id,
			component_daypart_id,
			hh_eq_cpm_start,
			hh_eq_cpm_end;

	-- pull only relevant load_forecasts data into memory based on the context of the proposal, and put the load_forecast into similar dimensions
	DECLARE @intersecting_load_forecast TABLE (media_month_id SMALLINT NOT NULL, media_week_id INT NOT NULL, network_id INT NOT NULL, component_daypart_id INT NOT NULL, hh_eq_cpm_start MONEY NOT NULL, hh_eq_cpm_end MONEY NOT NULL, subscribers INT NOT NULL, PRIMARY KEY CLUSTERED(media_month_id,media_week_id,network_id,component_daypart_id,hh_eq_cpm_start,hh_eq_cpm_end));
	INSERT INTO @intersecting_load_forecast
		SELECT
			mpcd.media_month_id,
			mpcd.media_week_id,
			mpcd.network_id,
			mpcd.component_daypart_id,
			mpcd.hh_eq_cpm_start,
			mpcd.hh_eq_cpm_end,
			SUM(ilf.subscribers)
		FROM
			@aggregated_media_plan_competitions mpcd
			JOIN dbo.inventory_load_forecasts ilf (NOLOCK) ON ilf.base_media_month_id=@latest_base_media_month_id
				AND ilf.forecast_media_month_id=mpcd.media_month_id
				AND ilf.forecast_media_week_id=mpcd.media_week_id
				AND ilf.network_id=mpcd.network_id
				AND ilf.component_daypart_id=mpcd.component_daypart_id
				AND ilf.hh_eq_cpm_start=mpcd.hh_eq_cpm_start
				AND ilf.hh_eq_cpm_end=mpcd.hh_eq_cpm_end
		GROUP BY
			mpcd.media_month_id,
			mpcd.media_week_id,
			mpcd.network_id,
			mpcd.component_daypart_id,
			mpcd.hh_eq_cpm_start,
			mpcd.hh_eq_cpm_end;

	-- join the load forecast to our combined dimensions
	INSERT INTO @calculated_load_forecast
		SELECT
			mpcd.media_month_id,
			mpcd.media_week_id,
			mpcd.network_id,
			mpcd.component_daypart_id,
			mpcd.hh_eq_cpm_start,
			mpcd.hh_eq_cpm_end,
			CASE WHEN mpcd.subscribers > ((lsl.alpha * mpcd.subscribers) + ((1-lsl.alpha)*ISNULL(lf.subscribers,0))) THEN
				mpcd.subscribers
			ELSE
				(lsl.alpha * mpcd.subscribers) + ((1-lsl.alpha)*ISNULL(lf.subscribers,0))
			END 'subscribers'
		FROM 
			@aggregated_media_plan_competitions mpcd
			JOIN @component_dayparts cd ON cd.daypart_id=mpcd.component_daypart_id
			LEFT JOIN @intersecting_load_forecast lf ON lf.media_month_id=mpcd.media_month_id 
				AND lf.media_week_id=mpcd.media_week_id 
				AND lf.network_id=mpcd.network_id 
				AND lf.component_daypart_id=mpcd.component_daypart_id 
				AND lf.hh_eq_cpm_start=mpcd.hh_eq_cpm_start 
				AND lf.hh_eq_cpm_end=mpcd.hh_eq_cpm_end
			JOIN @load_settings_log lsl ON lsl.forecast_media_month_id=mpcd.media_month_id
				AND lsl.network_id=mpcd.network_id
				AND lsl.week_part=cd.week_part;

	SELECT
		mpc.proposal_id 'ProposalId',
		mpc.unique_proposal_detail_id 'UniqueProposalDetailId',
		mpc.media_month_id 'MediaMonthId',
		mpc.media_week_id 'MediaWeekId',
		mpc.network_id 'NetworkId',
		mpc.component_daypart_id 'ComponentDaypartId',
		mpc.proposal_status_id 'ProposalStatus',
		mpc.hh_eq_cpm_start 'HhEqCpmStart',
		mpc.hh_eq_cpm_end 'HhEqCpmEnd',
		mpc.subscribers 'ContractedSubscribers'
	FROM
		@media_plan_competition mpc

	UNION

	-- subtract existing load from final mixed load forecast to get only forecasted portion
	SELECT
		10000 'ProposalId',
		CAST(ROW_NUMBER() OVER(ORDER BY r.media_month_id,r.media_week_id,r.network_id,r.component_daypart_id,r.hh_eq_cpm_start,r.hh_eq_cpm_end) AS VARCHAR) 'UniqueProposalDetailId',
		r.media_month_id 'MediaMonthId',
		r.media_week_id 'MediaWeekId',
		r.network_id 'NetworkId',
		r.component_daypart_id 'ComponentDaypartId',
		1 'ProposalStatus',
		r.hh_eq_cpm_start 'HhEqCpmStart',
		r.hh_eq_cpm_end 'HhEqCpmEnd',
		r.subscribers - mpcd.subscribers 'subscribers'
	FROM
		@calculated_load_forecast r
		JOIN @aggregated_media_plan_competitions mpcd ON mpcd.media_month_id=r.media_month_id
			AND mpcd.media_week_id=r.media_week_id
			AND mpcd.network_id=r.network_id
			AND mpcd.component_daypart_id=r.component_daypart_id
			AND mpcd.hh_eq_cpm_start=r.hh_eq_cpm_start
			AND mpcd.hh_eq_cpm_end=r.hh_eq_cpm_end
	WHERE
		r.subscribers - mpcd.subscribers>0
END