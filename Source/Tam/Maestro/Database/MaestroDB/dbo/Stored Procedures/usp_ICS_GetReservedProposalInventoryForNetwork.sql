-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/16/2015
-- Modified:	6/6/2016 - 0.25 CPM Bins
-- Description:	Returns reserved inventory specific to the networka nd media month specified
-- =============================================
/*
	DECLARE @media_month_id SMALLINT = 409
	DECLARE @network_id INT = 3
	EXEC usp_ICS_GetReservedProposalInventoryForNetwork @media_month_id, @network_id
*/
CREATE PROCEDURE [dbo].[usp_ICS_GetReservedProposalInventoryForNetwork]
	@media_month_id SMALLINT,
	@network_id INT,
	@apply_load_forecast BIT = 0
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
	
	DECLARE @proposals_on_avail_planner TABLE (proposal_id INT NOT NULL, proposal_status_id INT NOT NULL, PRIMARY KEY (proposal_id))
	INSERT INTO @proposals_on_avail_planner
		SELECT
			p.id,
			p.proposal_status_id
		FROM
			proposals p
			JOIN proposal_details pd ON pd.proposal_id=p.id
				AND pd.network_id=@network_id
			JOIN proposal_detail_worksheets pdw ON pdw.proposal_detail_id=pd.id
				AND pdw.units>0
			JOIN media_weeks mw ON mw.id=pdw.media_week_id
				AND mw.media_month_id=@media_month_id
		WHERE
			p.include_on_availability_planner=1
			AND p.total_gross_cost>0 -- filter out zero cost plans
		GROUP BY
			p.id,
			p.proposal_status_id
			
	DECLARE @unique_daypart_ids TABLE (source_daypart_id INT NOT NULL, PRIMARY KEY (source_daypart_id));
	INSERT INTO @unique_daypart_ids
		SELECT
			pd.daypart_id
		FROM
			@proposals_on_avail_planner p
			JOIN proposal_details pd ON pd.proposal_id=p.proposal_id
				AND pd.network_id=@network_id
		GROUP BY
			pd.daypart_id

	DECLARE @component_daypart_breakouts TABLE (source_daypart_id INT NOT NULL, component_daypart_id INT NOT NULL, component_hours FLOAT NOT NULL, total_component_hours FLOAT NOT NULL, PRIMARY KEY (source_daypart_id, component_daypart_id));
	INSERT INTO @component_daypart_breakouts
		SELECT DISTINCT
			ud.source_daypart_id,
			cd.id 'component_daypart_id',
			dbo.GetIntersectingDaypartHours(d.start_time,d.end_time, dc.start_time,dc.end_time) * dbo.GetIntersectingDaypartDays(d.mon,d.tue,d.wed,d.thu,d.fri,d.sat,d.sun, dc.mon,dc.tue,dc.wed,dc.thu,dc.fri,dc.sat,dc.sun) 'component_hours',
			d.total_hours 'total_component_hours'
		FROM
			@unique_daypart_ids ud
			JOIN vw_ccc_daypart d ON d.id=ud.source_daypart_id
			CROSS APPLY dbo.udf_GetIntersectingInventoryComponentDayparts(d.start_time,d.end_time,d.mon,d.tue,d.wed,d.thu,d.fri,d.sat,d.sun) cd
			JOIN vw_ccc_daypart dc ON dc.id=cd.id
		
	-- all EQ HH CPMs of all contracts in month
	DECLARE @cpms_by_unique_proposal_detail_id TABLE (unique_proposal_detail_id VARCHAR(63) NOT NULL, cpm_eq MONEY NOT NULL, PRIMARY KEY (unique_proposal_detail_id));
	INSERT INTO @cpms_by_unique_proposal_detail_id
		SELECT
			CAST(pd.id AS VARCHAR),
			CASE 
				WHEN (pda.us_universe * pd.universal_scaling_factor * pda.rating) > 0.0 THEN
					ROUND(CAST(pd.proposal_rate / (((pda.us_universe * pd.universal_scaling_factor * pda.rating) / 1000.0) * sl.delivery_multiplier) AS MONEY), 2)
				ELSE
					0.0
			END 'eq_hh_cpm'
		FROM
			@proposals_on_avail_planner p
			JOIN proposal_details pd ON pd.proposal_id=p.proposal_id
				AND pd.network_id=@network_id
			JOIN proposal_detail_audiences pda ON pda.proposal_detail_id=pd.id
				AND pda.audience_id=31
			JOIN spot_lengths sl (NOLOCK) ON sl.id=pd.spot_length_id

	-- CPM bins in increments of 0.25
	DECLARE @min_cpm MONEY;
	DECLARE @max_cpm MONEY;
	DECLARE @cpm_bin_increment MONEY = 0.25;
	SELECT @min_cpm=MIN(ROUND(cpm_eq,2)), @max_cpm=MAX(ROUND(cpm_eq,2)) FROM @cpms_by_unique_proposal_detail_id
	-- take CPM down until it hits an even increment of @cpm_bin_increment	
	WHILE (@min_cpm % @cpm_bin_increment) <> 0.00
		SET @min_cpm = @min_cpm-0.01;
	-- take CPM up until it hits an even increment of @cpm_bin_increment
	WHILE (@max_cpm % @cpm_bin_increment) <> 0.00
		SET @max_cpm = @max_cpm+0.01;

	-- create CPM bins
	DECLARE @cpm_bins TABLE (cpm_eq_start MONEY NOT NULL, cpm_eq_end MONEY NOT NULL)
	DECLARE @current_bin MONEY;
	SET @current_bin = @min_cpm;
	WHILE @current_bin <= @max_cpm
	BEGIN
		INSERT INTO @cpm_bins (cpm_eq_start, cpm_eq_end) VALUES (@current_bin, @current_bin+@cpm_bin_increment);
		SET @current_bin = @current_bin + @cpm_bin_increment;
	END

	DECLARE @contract_details TABLE (proposal_id INT NOT NULL, unique_proposal_detail_id VARCHAR(63) NOT NULL, daypart_id INT NOT NULL, media_week_id INT NOT NULL, media_month_id SMALLINT NOT NULL, proposal_status_id INT NOT NULL, network_id INT NOT NULL, contracted_subscribers BIGINT NOT NULL)
	INSERT INTO @contract_details
		SELECT
			pd.proposal_id,
			CAST(pd.id AS VARCHAR),
			pd.daypart_id,
			pdw.media_week_id,
			mw.media_month_id,
			p.proposal_status_id,
			pd.network_id,
			SUM(CAST(pd.topography_universe * pdw.units * sl.delivery_multiplier AS BIGINT)) 'contracted_subscribers'
		FROM
			@proposals_on_avail_planner p
			JOIN proposal_details pd ON pd.proposal_id=p.proposal_id
				AND pd.network_id=@network_id
			JOIN spot_lengths sl ON sl.id=pd.spot_length_id
			JOIN proposal_detail_worksheets pdw ON pdw.proposal_detail_id=pd.id
			JOIN media_weeks mw ON mw.id=pdw.media_week_id
				AND mw.media_month_id=@media_month_id
		WHERE
			pdw.units>0
			AND pd.topography_universe>0
		GROUP BY
			pd.proposal_id,
			pd.id,
			pd.daypart_id,
			pdw.media_week_id,
			mw.media_month_id,
			p.proposal_status_id,
			pd.network_id
		ORDER BY
			pd.proposal_id,
			pd.id,
			pd.daypart_id,
			pdw.media_week_id
		
	DECLARE @media_plan_competition MediaPlanCompetitionTable
	INSERT INTO @media_plan_competition
		SELECT
			cd.proposal_id 'ProposalId',
			cd.unique_proposal_detail_id 'UniqueProposalDetailId',
			cd.media_month_id 'MediaMonthId',
			cd.media_week_id 'MediaWeekId',
			cd.network_id 'NetworkId',
			cdb.component_daypart_id 'ComponentDaypartId',
			cd.proposal_status_id 'ProposalStatus',
			b.cpm_eq_start 'HhEqCpmStart',
			b.cpm_eq_end 'HhEqCpmEnd',
			CAST(SUM(cd.contracted_subscribers * (cdb.component_hours / cdb.total_component_hours)) AS BIGINT) 'ContractedSubscribers'
		FROM
			@contract_details cd
			JOIN @component_daypart_breakouts cdb ON cdb.source_daypart_id=cd.daypart_id
			JOIN @cpms_by_unique_proposal_detail_id pdcpm ON pdcpm.unique_proposal_detail_id=cd.unique_proposal_detail_id
			JOIN @cpm_bins b ON pdcpm.cpm_eq >= b.cpm_eq_start AND pdcpm.cpm_eq < b.cpm_eq_end
		GROUP BY
			cd.proposal_id,
			cd.unique_proposal_detail_id,
			cd.media_month_id,
			cd.media_week_id,
			cd.network_id,
			cdb.component_daypart_id,
			cd.proposal_status_id,
			b.cpm_eq_start,
			b.cpm_eq_end;

	IF @apply_load_forecast = 1
	BEGIN
		EXEC [dbo].[usp_ICS_ApplyInventoryLoadForecast] @media_plan_competition
	END
	ELSE
	BEGIN
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
	END
END