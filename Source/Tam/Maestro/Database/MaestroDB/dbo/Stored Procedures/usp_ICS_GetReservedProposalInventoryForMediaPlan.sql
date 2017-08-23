-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/9/2015
-- Modified:	5/6/2016 - updated to support non-committed media plan changes for the plan passed in.
--				6/6/2016 - 0.25 CPM Bins
-- Description:	Returns reserved inventory specific to the dimensions of the media plan line items (networks).
--				@inventory_details defines a dimension specific set of data to filter the result by. 
--				@proposal_inventory_details defines details specific to the media plan.
-- =============================================
/*
	DECLARE @proposal_id INT = 73475
	DECLARE @inventory_details InventoryRequestTable
	DECLARE @proposal_inventory_details ProposalInventoryRequestTable

	INSERT INTO @inventory_details (media_month_id,media_week_id,network_id,daypart_id,hh_eq_cpm,contracted_subscribers) 
		SELECT
			mw.media_month_id,
			mw.id,
			pd.network_id,
			pd.daypart_id,
			ROUND(dbo.GetProposalDetailCPMEquivalized(pd.id,31),2) 'hh_eq_cpm',
			SUM(CAST(pdw.units * pd.topography_universe AS BIGINT)) 'contracted_subscribers'
		FROM
			proposal_details pd (NOLOCK)
			JOIN proposal_detail_worksheets pdw (NOLOCK) ON pdw.proposal_detail_id=pd.id
			JOIN media_weeks mw (NOLOCK) ON mw.id=pdw.media_week_id
		WHERE
			pd.proposal_id=@proposal_id
			AND pdw.units>0
			AND pd.topography_universe>0
		GROUP BY
			mw.media_month_id,
			mw.id,
			pd.network_id,
			pd.daypart_id,
			ROUND(dbo.GetProposalDetailCPMEquivalized(pd.id,31),2)

	INSERT INTO @proposal_inventory_details (unique_proposal_detail_id,media_month_id,media_week_id,network_id,daypart_id,hh_eq_cpm,contracted_hh_coverage_universe,contracted_units,contracted_subscribers) 
		SELECT
			CAST(pd.id AS VARCHAR) 'unique_proposal_detail_id',
			mw.media_month_id,
			mw.id,
			pd.network_id,
			pd.daypart_id,
			ROUND(dbo.GetProposalDetailCPMEquivalized(pd.id,31),2) 'hh_eq_cpm',
			pd.topography_universe 'contracted_hh_coverage_universe',
			SUM(pdw.units * 3) 'contracted_units',
			SUM(CAST(pdw.units * pd.topography_universe AS BIGINT)) 'contracted_subscribers'
		FROM
			proposal_details pd (NOLOCK)
			JOIN proposal_detail_worksheets pdw (NOLOCK) ON pdw.proposal_detail_id=pd.id
			JOIN media_weeks mw (NOLOCK) ON mw.id=pdw.media_week_id
		WHERE
			pd.proposal_id=@proposal_id
			AND pdw.units>0
			AND pd.topography_universe>0
		GROUP BY
			pd.id,
			mw.media_month_id,
			mw.id,
			pd.network_id,
			pd.daypart_id,
			pd.topography_universe,
			ROUND(dbo.GetProposalDetailCPMEquivalized(pd.id,31),2)
					
	EXEC usp_ICS_GetReservedProposalInventoryForMediaPlan @proposal_id, @inventory_details, @proposal_inventory_details,0
	EXEC usp_ICS_GetReservedProposalInventoryForMediaPlan @proposal_id, @inventory_details, @proposal_inventory_details,1
*/
CREATE PROCEDURE [dbo].[usp_ICS_GetReservedProposalInventoryForMediaPlan]
	@proposal_id INT,
	@inventory_details InventoryRequestTable READONLY,
	@proposal_inventory_details ProposalInventoryRequestTable READONLY,
	@apply_load_forecast BIT = 0
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	DECLARE @inventory_filter_dayparts TABLE (source_daypart_id INT NOT NULL, component_daypart_id INT NOT NULL,  component_hours FLOAT NOT NULL, total_component_hours FLOAT NOT NULL, PRIMARY KEY (source_daypart_id, component_daypart_id));
	INSERT INTO @inventory_filter_dayparts
		SELECT DISTINCT
			pd.daypart_id,
			cd.id,
			dbo.GetIntersectingDaypartHours(d.start_time,d.end_time, dc.start_time,dc.end_time) * dbo.GetIntersectingDaypartDays(d.mon,d.tue,d.wed,d.thu,d.fri,d.sat,d.sun, dc.mon,dc.tue,dc.wed,dc.thu,dc.fri,dc.sat,dc.sun) 'component_hours',
			d.total_hours 'total_component_hours'
		FROM
			@inventory_details pd
			JOIN vw_ccc_daypart d ON d.id=pd.daypart_id
			CROSS APPLY dbo.udf_GetIntersectingInventoryComponentDayparts(d.start_time,d.end_time,d.mon,d.tue,d.wed,d.thu,d.fri,d.sat,d.sun) cd
			JOIN vw_ccc_daypart dc ON dc.id=cd.id
					
	DECLARE @inventory_filter_networks_and_media_weeks TABLE (media_week_id INT NOT NULL, network_id INT NOT NULL, media_month_id INT NOT NULL, PRIMARY KEY (media_week_id, network_id, media_month_id));
	INSERT INTO @inventory_filter_networks_and_media_weeks
		SELECT
			pd.media_week_id,
			pd.network_id,
			pd.media_month_id
		FROM
			@inventory_details pd
		GROUP BY
			pd.media_week_id,
			pd.network_id,
			pd.media_month_id

	DECLARE @inventory_filter_networks_media_weeks_and_component_dayparts TABLE (media_week_id INT NOT NULL, network_id INT NOT NULL, component_daypart_id INT NOT NULL, PRIMARY KEY (media_week_id, network_id, component_daypart_id));
	INSERT INTO @inventory_filter_networks_media_weeks_and_component_dayparts
		SELECT
			pd.media_week_id,
			pd.network_id,
			ifd.component_daypart_id
		FROM
			@inventory_details pd
			JOIN @inventory_filter_dayparts ifd ON ifd.source_daypart_id=pd.daypart_id
		GROUP BY
			pd.media_week_id,
			pd.network_id,
			ifd.component_daypart_id

	DECLARE @proposals_on_avail_planner TABLE (proposal_id INT NOT NULL, proposal_status_id INT NOT NULL, PRIMARY KEY (proposal_id))
	INSERT INTO @proposals_on_avail_planner
		SELECT
			p.id,
			p.proposal_status_id
		FROM
			proposals p
			JOIN proposal_details pd ON pd.proposal_id=p.id
			JOIN proposal_detail_worksheets pdw ON pdw.proposal_detail_id=pd.id
				AND pdw.units>0
			JOIN @inventory_filter_networks_and_media_weeks ifnmw ON ifnmw.media_week_id=pdw.media_week_id
				AND ifnmw.network_id=pd.network_id
		WHERE
			p.include_on_availability_planner=1 -- only proposals explicitly put on the avail planner
			AND p.id<>@proposal_id
			AND p.total_gross_cost>0 -- filter out zero cost plans
		GROUP BY
			p.id,
			proposal_status_id
			
	DECLARE @unique_daypart_ids TABLE (source_daypart_id INT NOT NULL, PRIMARY KEY (source_daypart_id));
	INSERT INTO @unique_daypart_ids
		SELECT
			pd.daypart_id
		FROM
			@proposals_on_avail_planner p
			JOIN proposal_details pd ON pd.proposal_id=p.proposal_id
		GROUP BY
			pd.daypart_id

		UNION

		SELECT
			pd.daypart_id
		FROM
			@inventory_details pd
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
			
	DECLARE @cpms_by_unique_proposal_detail_id TABLE (unique_proposal_detail_id VARCHAR(63) NOT NULL, cpm_eq MONEY NOT NULL, PRIMARY KEY (unique_proposal_detail_id));
	-- all EQ HH CPMs of all contracts in month (except proposal passed in)
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
			JOIN proposal_detail_audiences pda ON pda.proposal_detail_id=pd.id
				AND pda.audience_id=31
			JOIN spot_lengths sl (NOLOCK) ON sl.id=pd.spot_length_id;

	-- all EQ HH CPMs of only proposal passed in
	INSERT INTO @cpms_by_unique_proposal_detail_id
		SELECT
			pid.unique_proposal_detail_id,
			ROUND(pid.hh_eq_cpm,2)
		FROM
			@proposal_inventory_details pid
		GROUP BY
			pid.unique_proposal_detail_id,
			pid.hh_eq_cpm

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
	-- all proposals except the one passed in
	INSERT INTO @contract_details
		SELECT
			pd.proposal_id,
			CAST(pd.id AS VARCHAR),
			pd.daypart_id,
			pdw.media_week_id,
			ifmw.media_month_id,
			p.proposal_status_id,
			pd.network_id,
			SUM(CAST(pd.topography_universe * pdw.units * sl.delivery_multiplier AS BIGINT)) 'contracted_subscribers'
		FROM
			@proposals_on_avail_planner p
			JOIN proposal_details pd ON pd.proposal_id=p.proposal_id
			JOIN spot_lengths sl ON sl.id=pd.spot_length_id
			JOIN proposal_detail_worksheets pdw ON pdw.proposal_detail_id=pd.id
			JOIN @inventory_filter_networks_and_media_weeks ifmw ON ifmw.media_week_id=pdw.media_week_id
				AND ifmw.network_id=pd.network_id
		WHERE
			pdw.units>0
			AND pd.topography_universe>0
		GROUP BY
			pd.proposal_id,
			pd.id,
			pd.daypart_id,
			pdw.media_week_id,
			ifmw.media_month_id,
			p.proposal_status_id,
			pd.network_id
		ORDER BY
			pd.proposal_id,
			pd.id,
			pd.daypart_id,
			pdw.media_week_id
	
	-- the proposal passed in only
	INSERT INTO @contract_details
		SELECT
			@proposal_id,
			pid.unique_proposal_detail_id,
			pid.daypart_id,
			pid.media_week_id,
			pid.media_month_id,
			p.proposal_status_id,
			pid.network_id,
			pid.contracted_subscribers
		FROM
			@proposal_inventory_details pid
			JOIN proposals p ON p.id=@proposal_id
		WHERE
			pid.contracted_hh_coverage_universe>0
			AND pid.contracted_units>0
		ORDER BY
			pid.unique_proposal_detail_id,
			pid.daypart_id,
			pid.media_week_id
	
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
			JOIN @inventory_filter_networks_media_weeks_and_component_dayparts invf ON invf.network_id=cd.network_id
				AND invf.media_week_id=cd.media_week_id
				AND invf.component_daypart_id=cdb.component_daypart_id
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