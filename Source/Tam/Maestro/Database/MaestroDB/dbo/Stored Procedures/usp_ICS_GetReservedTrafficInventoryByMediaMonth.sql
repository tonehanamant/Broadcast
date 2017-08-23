-- =============================================
-- Author:      Stephen DeFusco
-- Create date: 7/27/2016
-- Description: Get's all reserved traffic inventory for a media month.
-- =============================================
-- EXEC usp_ICS_GetReservedTrafficInventoryByMediaMonth 425,'82948,82949,83108,83492,84201,84210,84497,84703,84704,84826,85711,86468,87977,88633,88665,88870,88871,88874,88875,88876,88877,84539,84540,85083'
-- EXEC usp_ICS_GetReservedTrafficInventoryByMediaMonth 420,''
CREATE PROCEDURE [dbo].[usp_ICS_GetReservedTrafficInventoryByMediaMonth]
       @media_month_id INT,
	   @proposal_dashboard_ids VARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	DECLARE @media_month_start DATETIME;
	DECLARE @media_month_end DATETIME;
	DECLARE @current_date DATE = CAST(GETDATE() AS DATE);
	DECLARE @current_media_month_start DATETIME;
	DECLARE @current_media_month_end DATETIME;

	SELECT
		@media_month_start = mm.start_date,
		@media_month_end = mm.end_date
	FROM
		media_months mm
	WHERE
		mm.id=@media_month_id;

	SELECT
		@current_media_month_start = mm.start_date,
		@current_media_month_end = mm.end_date
	FROM
		media_months mm
	WHERE
		@current_date BETWEEN mm.start_date AND mm.end_date;

	DECLARE @traffic_released_ids_released_this_month TABLE (traffic_id INT NOT NULL);
	DECLARE @traffic_released_ids_not_yet_released_this_month TABLE (traffic_id INT NOT NULL);

	INSERT INTO @traffic_released_ids_released_this_month
		SELECT DISTINCT 
			tro.traffic_id 
		FROM 
			traffic_orders tro 
		WHERE
			tro.media_month_id=@media_month_id;

	INSERT INTO @traffic_released_ids_not_yet_released_this_month
		SELECT DISTINCT 
			tstag.traffic_id 
		FROM 
			traffic_spot_targets tst
			JOIN traffic_spot_target_allocation_group tstag ON tstag.id=tst.traffic_spot_target_allocation_group_id
		WHERE
			(tstag.start_date<=@media_month_end AND tstag.end_date>=@media_month_start)
			AND tstag.traffic_id NOT IN (
				SELECT traffic_id FROM @traffic_released_ids_released_this_month
			) 
              
	DECLARE @traffic_released TABLE (traffic_id INT NOT NULL, traffic_spot_target_id INT NOT NULL, media_month_id INT NOT NULL, media_week_id INT NOT NULL, business_id INT NOT NULL, daypart_id INT NOT NULL, network_id INT NOT NULL, subscribers BIGINT NOT NULL, cost MONEY NOT NULL, hh_eq_impressions FLOAT NOT NULL, hh_eq_cpm MONEY NOT NULL, units FLOAT NOT NULL);
	INSERT INTO @traffic_released
		SELECT
			td.traffic_id,
			tst.id,
			tro.media_month_id,
			tro.media_week_id,
			tstag.mvpd_id 'business_id',
			tst.daypart_id,
			td.network_id,
			SUM(CAST(tro.ordered_spots AS BIGINT) * CAST(tro.subscribers * sl.delivery_multiplier AS BIGINT)) 'total_subscribers',
			SUM(tro.ordered_spots * tro.ordered_spot_rate) 'total_cost',
			SUM(CAST(tro.ordered_spots AS BIGINT) * CAST(tro.subscribers AS BIGINT) * tsta.traffic_rating * sl.delivery_multiplier) 'hh_eq_impressions',
			ROUND(
				SUM(tro.ordered_spot_rate) / (SUM(CAST(tro.subscribers AS BIGINT) * tsta.traffic_rating * sl.delivery_multiplier)/1000.0)
			,2) 'hh_eq_cpm',
			SUM(CAST(tro.ordered_spots AS BIGINT) * CAST(tro.subscribers AS BIGINT) * sl.delivery_multiplier) / tstag.subscribers 'units'
		FROM
			traffic_orders tro
			JOIN traffic_details td ON td.id=tro.traffic_detail_id
			JOIN traffic_spot_targets tst ON tst.id=tro.traffic_spot_target_id
			JOIN traffic_spot_target_audiences tsta ON tsta.traffic_spot_target_id=tst.id
			AND tsta.audience_id=31
			JOIN traffic_spot_target_allocation_group tstag ON tstag.id=tst.traffic_spot_target_allocation_group_id
			JOIN spot_lengths sl ON sl.id=td.spot_length_id
		WHERE
			tro.media_month_id=@media_month_id
			AND tro.on_financial_reports=1
			AND tro.active=1
			AND tro.ordered_spots>0
			AND tro.ordered_spot_rate>0
			AND tst.suspended=0
		GROUP BY
			td.traffic_id,
			tst.id,
			tro.media_month_id,
			tro.media_week_id,
			tstag.mvpd_id,
			tst.daypart_id,
			td.network_id,
			tstag.subscribers
		HAVING
			SUM(tro.subscribers * tsta.traffic_rating * sl.delivery_multiplier)>0;

	DECLARE @traffic_not_released TABLE (traffic_id INT NOT NULL, traffic_spot_target_id INT NOT NULL, media_month_id INT NOT NULL, media_week_id INT NOT NULL, business_id INT NOT NULL, daypart_id INT NOT NULL, network_id INT NOT NULL, subscribers BIGINT NOT NULL, cost MONEY NOT NULL, hh_eq_impressions FLOAT NOT NULL, hh_eq_cpm MONEY NOT NULL, units FLOAT NOT NULL);
	INSERT INTO @traffic_not_released
		SELECT
			tst.traffic_id,
			tst.id,
			mw.media_month_id,
			mw.id 'media_week_id',
			tstag.mvpd_id 'business_id',
			tst.daypart_id,
			td.network_id,
			CAST(tst.spots AS BIGINT) * CAST(tstag.subscribers * sl.delivery_multiplier AS BIGINT) 'total_subscribers',
			tst.spots * tst.spot_cost 'total_cost',
			tst.spots * tsta.impressions_per_spot * sl.delivery_multiplier 'hh_eq_impressions',
			ROUND(
				tst.spot_cost / ((CAST(tstag.subscribers AS BIGINT) * tsta.traffic_rating * sl.delivery_multiplier)/1000.0)
			,2) 'hh_eq_cpm',
			CAST(tst.spots AS FLOAT) * sl.delivery_multiplier 'units'
		FROM
			traffic_spot_target_allocation_group tstag
			JOIN @traffic_released_ids_not_yet_released_this_month tids ON tids.traffic_id=tstag.traffic_id
			JOIN traffic_spot_targets tst ON tst.traffic_spot_target_allocation_group_id=tstag.id
			JOIN traffic_spot_target_audiences tsta ON tsta.traffic_spot_target_id=tst.id
			AND tsta.audience_id=31
			JOIN traffic_details td ON td.id=tstag.traffic_detail_id
			JOIN spot_lengths sl ON sl.id=td.spot_length_id
			JOIN media_weeks mw ON tstag.start_date BETWEEN mw.start_date AND mw.end_date
				AND mw.media_month_id=@media_month_id
		WHERE
			tst.suspended=0
			AND (tst.spots * tsta.impressions_per_spot * sl.delivery_multiplier)>0
			AND (tstag.subscribers * tsta.traffic_rating * sl.delivery_multiplier)>0;

	--DECLARE @traffic_load_forecast TABLE (media_month_id SMALLINT NOT NULL, media_week_id INT NOT NULL, business_id INT NOT NULL, network_id INT NOT NULL, daypart_id INT NOT NULL, hh_eq_cpm MONEY NOT NULL, subscribers BIGINT NOT NULL, units FLOAT NOT NULL);
	--IF @media_month_start > @current_media_month_start
	--BEGIN
	--	INSERT INTO @traffic_load_forecast
	--		EXEC dbo.usp_ICS_GetProposalSubscribersExpectedToBeTraffickedButHaveNotBeenYet @media_month_id, @proposal_dashboard_ids;
	--END

	DECLARE @unique_daypart_ids TABLE (source_daypart_id INT NOT NULL, PRIMARY KEY (source_daypart_id));
	INSERT INTO @unique_daypart_ids
		SELECT t.daypart_id FROM @traffic_released t GROUP BY t.daypart_id
		UNION
		SELECT t.daypart_id FROM @traffic_not_released t GROUP BY t.daypart_id
		--UNION
		--SELECT t.daypart_id FROM @traffic_load_forecast t GROUP BY t.daypart_id;

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
			JOIN vw_ccc_daypart dc ON dc.id=cd.id;

	DECLARE @unique_hh_eq_cpms TABLE (hh_eq_cpm MONEY);
	INSERT INTO @unique_hh_eq_cpms
		SELECT ROUND(hh_eq_cpm,2) FROM @traffic_released t GROUP BY ROUND(hh_eq_cpm,2)
		UNION
		SELECT ROUND(hh_eq_cpm,2) FROM @traffic_not_released t GROUP BY ROUND(hh_eq_cpm,2)
		--UNION
		--SELECT ROUND(hh_eq_cpm,2) FROM @traffic_load_forecast t GROUP BY ROUND(hh_eq_cpm,2);

	-- CPM bins in increments of 0.25
	DECLARE @min_cpm MONEY;
	DECLARE @max_cpm MONEY;
	DECLARE @cpm_bin_increment MONEY = 0.25;
	SELECT @min_cpm=MIN(ROUND(hh_eq_cpm,2)), @max_cpm=MAX(ROUND(hh_eq_cpm,2)) FROM @unique_hh_eq_cpms
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


	SELECT
		t.traffic_id,
		t.traffic_spot_target_id,
		t.media_month_id,
		t.media_week_id,
		t.business_id,
		t.network_id,
		cdb.component_daypart_id,
		b.cpm_eq_start,
		b.cpm_eq_end,
		SUM(CAST(t.subscribers * (cdb.component_hours / cdb.total_component_hours) AS BIGINT)) 'subscribers',
		SUM(t.units * (cdb.component_hours / cdb.total_component_hours)) 'units'
	FROM
		@traffic_released t
		JOIN @component_daypart_breakouts cdb ON cdb.source_daypart_id=t.daypart_id
		JOIN @cpm_bins b ON t.hh_eq_cpm >= b.cpm_eq_start AND t.hh_eq_cpm < b.cpm_eq_end
	GROUP BY
		t.traffic_id,
		t.traffic_spot_target_id,
		t.media_month_id,
		t.media_week_id,
		t.business_id,
		t.network_id,
		cdb.component_daypart_id,
		b.cpm_eq_start,
		b.cpm_eq_end

	UNION ALL
       
	SELECT
		t.traffic_id,
		t.traffic_spot_target_id,
		t.media_month_id,
		t.media_week_id,
		t.business_id,
		t.network_id,
		cdb.component_daypart_id,
		b.cpm_eq_start,
		b.cpm_eq_end,
		SUM(CAST(t.subscribers * (cdb.component_hours / cdb.total_component_hours) AS BIGINT)) 'subscribers',
		SUM(t.units * (cdb.component_hours / cdb.total_component_hours)) 'units'
	FROM
		@traffic_not_released t
		JOIN @component_daypart_breakouts cdb ON cdb.source_daypart_id=t.daypart_id
		JOIN @cpm_bins b ON t.hh_eq_cpm >= b.cpm_eq_start AND t.hh_eq_cpm < b.cpm_eq_end
	GROUP BY
		t.traffic_id,
		t.traffic_spot_target_id,
		t.media_month_id,
		t.media_week_id,
		t.business_id,
		t.network_id,
		cdb.component_daypart_id,
		b.cpm_eq_start,
		b.cpm_eq_end

	--UNION ALL

	--SELECT
	--	CAST(0 AS INT),
	--	CAST(0 AS INT),
	--	CAST(t.media_month_id AS INT),
	--	t.media_week_id,
	--	t.business_id,
	--	t.network_id,
	--	cdb.component_daypart_id,
	--	b.cpm_eq_start,
	--	b.cpm_eq_end,
	--	SUM(CAST(t.subscribers * (cdb.component_hours / cdb.total_component_hours) AS BIGINT)) 'subscribers',
	--	SUM(t.units * (cdb.component_hours / cdb.total_component_hours)) 'units'
	--FROM
	--	@traffic_load_forecast t
	--	JOIN @component_daypart_breakouts cdb ON cdb.source_daypart_id=t.daypart_id
	--	JOIN @cpm_bins b ON t.hh_eq_cpm >= b.cpm_eq_start AND t.hh_eq_cpm < b.cpm_eq_end
	--GROUP BY
	--	t.media_month_id,
	--	t.media_week_id,
	--	t.business_id,
	--	t.network_id,
	--	cdb.component_daypart_id,
	--	b.cpm_eq_start,
	--	b.cpm_eq_end         
END