-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 08/26/2015
-- Modified:	08/08/2016 - Added aggregation of inventory.dbo.proposal_detail_delivery
-- Description:	Aggregates the unduplicated posted subscribers by media month into inventory_inputs and proposal_detail_delivery
-- =============================================
CREATE PROCEDURE [dbo].[usp_ICS_CrunchInventoryInputs]
	@media_month_id INT
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
	SET DATEFIRST 1; --MON

	DECLARE @media_month VARCHAR(4);
	
	SELECT @media_month = mm.media_month FROM media_months mm WHERE mm.id=@media_month_id;
	
	-- clear month if it already exists
	IF (SELECT COUNT(1) FROM inventory.dbo.inventory_inputs ii WHERE ii.media_month_id=@media_month_id) > 0
		DELETE FROM inventory.dbo.inventory_inputs WHERE media_month_id=@media_month_id;

	-- clear month if it already exists
	IF (SELECT COUNT(1) FROM inventory.dbo.inventory_inputs_final ii WHERE ii.media_month=@media_month) > 0
		DELETE FROM inventory.dbo.inventory_inputs_final WHERE media_month=@media_month;

	-- clear month if it already exists
	IF (SELECT COUNT(1) FROM inventory.dbo.proposal_detail_delivery pdd JOIN proposals p ON p.id=pdd.proposal_id WHERE p.posting_media_month_id=@media_month_id) > 0
		DELETE FROM inventory.dbo.proposal_detail_delivery FROM inventory.dbo.proposal_detail_delivery pdd JOIN proposals p ON p.id=pdd.proposal_id WHERE p.posting_media_month_id=@media_month_id;
	
	-- 8 components (3 hour blocks M-SU)
	DECLARE @component_dayparts TABLE (daypart_id INT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, mon BIT NOT NULL, tue BIT NOT NULL, wed BIT NOT NULL, thu BIT NOT NULL, fri BIT NOT NULL, sat BIT NOT NULL, sun BIT NOT NULL, total_hours INT NOT NULL);
	INSERT INTO @component_dayparts
		SELECT * FROM dbo.udf_Get48ComponentDayparts();
	
	-- get a list of unduplicated CADENT post results
	DECLARE @unduplicated_tam_post_proposal_ids TABLE (tam_post_proposal_id INT NOT NULL, proposal_id INT NOT NULL, rate_card_type_id TINYINT NOT NULL, guaranteed_audience_id INT NOT NULL);
	INSERT INTO @unduplicated_tam_post_proposal_ids
		SELECT
			MIN(tpp.id) 'tam_post_proposal_id',
			tpp.posting_plan_proposal_id,
			p.rate_card_type_id,
			pa.audience_id
		FROM
			tam_post_proposals tpp
			JOIN proposals p ON p.id=tpp.posting_plan_proposal_id
			JOIN tam_posts tp ON tp.id=tpp.tam_post_id
				AND tp.post_type_code=1 -- OFFICIAL
				AND tp.is_deleted=0
			JOIN dbo.proposal_audiences pa (NOLOCK) ON pa.proposal_id=p.id
				AND pa.ordinal=p.guarantee_type
		WHERE
			tpp.post_source_code=0 -- CADENT POST
			AND p.posting_media_month_id=@media_month_id
			AND tpp.post_completed IS NOT NULL
			AND tpp.aggregation_completed IS NOT NULL
		GROUP BY
			tpp.posting_plan_proposal_id,
			p.rate_card_type_id,
			pa.audience_id;
	
	-- all EQ HH CPMs of all contracts in month
	DECLARE @all_cpms_of_all_contracts_in_month TABLE (proposal_detail_id INT NOT NULL, cpm_eq MONEY NOT NULL);
	INSERT INTO @all_cpms_of_all_contracts_in_month
		SELECT
			pd.id,
			ROUND(dbo.GetProposalDetailCPMEquivalized(pd.id,31),2) -- HH EQ CPM
		FROM
			@unduplicated_tam_post_proposal_ids uttp
			JOIN proposal_details pd ON pd.proposal_id=uttp.proposal_id;
	
	DECLARE @end_date DATE;
	DECLARE @current_date DATE;
	SELECT
		@current_date = mm.start_date,
		@end_date = mm.end_date
	FROM
		dbo.media_months mm
	WHERE
		mm.id=@media_month_id;
	DECLARE @days_of_week TABLE (date_of DATE NOT NULL, week_day TINYINT NOT NULL)
	WHILE @current_date <= @end_date
	BEGIN
		INSERT INTO @days_of_week (date_of,week_day) VALUES (@current_date,DATEPART(WEEKDAY,@current_date));
		SET @current_date = DATEADD(DAY,1,@current_date);
	END
	
	-- CPM bins in increments of 0.25
	DECLARE @min_cpm MONEY;
	DECLARE @max_cpm MONEY;
	DECLARE @cpm_bin_increment MONEY = 0.25;
	SELECT @min_cpm=MIN(ROUND(cpm_eq,2)), @max_cpm=MAX(ROUND(cpm_eq,2)) FROM @all_cpms_of_all_contracts_in_month
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
	
	INSERT INTO inventory.dbo.inventory_inputs (media_month_id, media_week_id, business_id, daypart_id, network_id, hh_eq_cpm_start, hh_eq_cpm_end, subscribers, units, total_spots)
		SELECT
			tpa.media_month_id,
			tpa.media_week_id,
			tpa.business_id,
			cd.daypart_id,
			tpa.posted_network_id,
			b.cpm_eq_start,
			b.cpm_eq_end,
			SUM(tpa.subscribers * sl.delivery_multiplier) 'subscribers',
			SUM(tpa.units) 'units',
			COUNT(1) 'total_spots'
		FROM
			@unduplicated_tam_post_proposal_ids utpp
			JOIN maestro_analysis.dbo.tam_post_affidavits tpa ON tpa.media_month_id=@media_month_id
				AND tpa.tam_post_proposal_id=utpp.tam_post_proposal_id 
			JOIN dbo.materials m ON m.id=tpa.posted_material_id
			JOIN dbo.spot_lengths sl ON sl.id=m.spot_length_id
			JOIN @days_of_week dow ON dow.date_of=tpa.air_date
			JOIN @component_dayparts cd ON 1=CASE dow.week_day WHEN 1 THEN cd.mon WHEN 2 THEN cd.tue WHEN 3 THEN cd.wed WHEN 4 THEN cd.thu WHEN 5 THEN cd.fri WHEN 6 THEN cd.sat WHEN 7 THEN cd.sun END
				AND tpa.air_time BETWEEN cd.start_time AND cd.end_time
			JOIN @all_cpms_of_all_contracts_in_month pdcpm ON pdcpm.proposal_detail_id=tpa.proposal_detail_id
			JOIN @cpm_bins b ON pdcpm.cpm_eq >= b.cpm_eq_start AND pdcpm.cpm_eq < b.cpm_eq_end
		WHERE
			tpa.media_month_id=@media_month_id
		GROUP BY
			tpa.media_month_id,
			tpa.media_week_id,
			tpa.business_id,
			cd.daypart_id,
			tpa.posted_network_id,
			b.cpm_eq_start,
			b.cpm_eq_end;

	WITH t1 AS ( 
		SELECT 
			media_month,
			media_month_index,
			week_number,
			CASE WHEN d.mon = 1 THEN 0 ELSE 1 END 'week_part',
			FLOOR(d.end_time / 3600) * 3600 'seconds',
			network_id,
			business_id,
			CEILING(hh_eq_cpm_end / .25) * .25 'cpm',
			subscribers
		FROM 
			inventory.dbo.inventory_inputs ii
			JOIN vw_ccc_daypart d ON d.id=daypart_id
			JOIN inventory.dbo.media_months_ix mm ON media_month_id = mm.id
			JOIN media_weeks mw ON media_week_id = mw.id
		WHERE
			ii.media_month_id=@media_month_id
	)
	INSERT INTO inventory.dbo.inventory_inputs_final (media_month, media_month_index, week_number, week_part, seconds, network_id, business_id, cpm, subscribers)
		SELECT
			media_month, media_month_index, week_number, week_part, seconds, network_id, business_id, cpm, SUM(subscribers) 'subscribers' FROM t1
		GROUP BY 
			media_month, media_month_index, week_number, week_part, seconds, network_id, business_id, cpm


	DECLARE @contracted TABLE (media_month_id INT NOT NULL, proposal_detail_id INT NOT NULL, hh_impressions FLOAT NOT NULL, gd_impressions FLOAT)
	INSERT INTO @contracted
		SELECT
			p.posting_media_month_id,
			pd.id,
			dbo.GetProposalDetailTotalDeliverySpecifyEq(pd.id,31,1) * 1000.0,
			dbo.GetProposalDetailTotalDeliverySpecifyEq(pd.id,pa.audience_id,1) * 1000.0
		FROM
			dbo.proposals p
			JOIN dbo.proposal_details pd ON pd.proposal_id=p.id
			LEFT JOIN dbo.proposal_audiences pa ON pa.proposal_id=p.id AND pa.ordinal=p.guarantee_type
		WHERE
			p.id IN (SELECT proposal_id FROM @unduplicated_tam_post_proposal_ids)

	DECLARE @proposal_detail_delivery_summary TABLE (proposal_id INT NOT NULL, proposal_detail_id INT NOT NULL, hh_delivery FLOAT, guaranteed_delivery FLOAT, hh_delivery_percentage FLOAT, guaranteed_delivery_percentage FLOAT)
	INSERT INTO @proposal_detail_delivery_summary
		SELECT
			utpp.proposal_id,
			tpa.proposal_detail_id,
			SUM(CASE utpp.rate_card_type_id WHEN 1 THEN tpad.eq_delivery ELSE tpad.dr_eq_delivery END) 'hh_eq_delivery',
			SUM(CASE utpp.rate_card_type_id WHEN 1 THEN tpad_gd.eq_delivery ELSE tpad_gd.dr_eq_delivery END) 'gd_eq_delivery',
			NULL,
			NULL
		FROM
			maestro_analysis.dbo.tam_post_affidavits tpa
			JOIN @unduplicated_tam_post_proposal_ids utpp ON utpp.tam_post_proposal_id=tpa.tam_post_proposal_id
			JOIN maestro_analysis.dbo.tam_post_affidavit_details tpad ON tpad.media_month_id=tpa.media_month_id
				AND tpad.tam_post_proposal_id=tpa.tam_post_proposal_id
				AND tpad.tam_post_affidavit_id=tpa.id
				AND tpad.audience_id=31
			JOIN maestro_analysis.dbo.tam_post_affidavit_details tpad_gd ON tpad_gd.media_month_id=tpa.media_month_id
				AND tpad_gd.tam_post_proposal_id=tpa.tam_post_proposal_id
				AND tpad_gd.tam_post_affidavit_id=tpa.id
				AND tpad_gd.audience_id=utpp.guaranteed_audience_id
		WHERE
			tpa.media_month_id=@media_month_id
		GROUP BY
			utpp.proposal_id,
			tpa.proposal_detail_id

	UPDATE
		@proposal_detail_delivery_summary
	SET
		hh_delivery_percentage=CASE WHEN c.hh_impressions > 0 THEN pdds.hh_delivery / c.hh_impressions ELSE 0 END,
		guaranteed_delivery_percentage=CASE WHEN c.gd_impressions > 0 THEN pdds.guaranteed_delivery / c.gd_impressions ELSE 0 END
	FROM
		@proposal_detail_delivery_summary pdds
		JOIN @contracted c ON c.proposal_detail_id=pdds.proposal_detail_id

	INSERT INTO inventory.dbo.proposal_detail_delivery
		SELECT
			pdds.proposal_id,
			pdds.proposal_detail_id,
			pdds.hh_delivery,
			pdds.guaranteed_delivery,
			pdds.hh_delivery_percentage,
			pdds.guaranteed_delivery_percentage
		FROM
			@proposal_detail_delivery_summary pdds

	-- populate inventory.dbo.load_inputs and inventory.dbo.load_inputs_final
	EXEC dbo.usp_ICS_ProcessInventoryLoadInputs @media_month_id;
END