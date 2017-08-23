
/***************************************************************************************************
** Date         Author          Description   
** ---------    ----------		-----------------------------------------------------
** 2//2014		Nicholas Kheynis & Stephen DeFusco
** 03/13/2017	Abdul Sukkur	Added new columns to systems 
** 05/11/2017	Abdul Sukkur	Added column custom_traffic_system to systems 
*****************************************************************************************************/
-- usp_STS2_MissingMSOsForCurrentMonthAgainstPreviousMonth 398, 1
CREATE PROCEDURE [dbo].[usp_STS2_MissingMSOsForCurrentMonthAgainstPreviousMonth]
		@media_month_id INT,
		@business_unit_id INT
	AS
	BEGIN
		DECLARE @start_date DATETIME, @end_date DATETIME
		SELECT @start_date = mm.start_date, @end_date = mm.end_date FROM media_months mm WHERE mm.id=@media_month_id;

		IF OBJECT_ID('tempdb..#trafficked') IS NOT NULL DROP TABLE #trafficked;
		CREATE TABLE #trafficked (system_id INT)
		INSERT INTO #trafficked
			SELECT
				sz.system_id
			FROM
				traffic_orders tro (NOLOCK)
				JOIN traffic_details td (NOLOCK) ON td.id=tro.traffic_detail_id
				JOIN traffic t (NOLOCK) ON t.id=td.traffic_id
				JOIN uvw_systemzone_universe sz (NOLOCK) ON sz.zone_id=tro.zone_id
					AND (sz.start_date<=t.start_date AND (sz.end_date>=t.start_date OR sz.end_date IS NULL))
					AND sz.type='BILLING'
			WHERE
				(tro.start_date <= @end_date AND tro.end_date >= @start_date)
				AND tro.zone_id NOT IN (
					SELECT primary_zone_id FROM uvw_zonezone_universe zz WHERE (zz.start_date<=@start_date AND (zz.end_date>=@start_date OR zz.end_date IS NULL))
				)
			GROUP BY
				sz.system_id
		
			UNION
		
			SELECT
				sz.system_id
			FROM
				traffic_orders tro (NOLOCK)
				JOIN traffic_details td (NOLOCK) ON td.id=tro.traffic_detail_id
				JOIN traffic t (NOLOCK) ON t.id=td.traffic_id
				JOIN uvw_zonezone_universe zz (NOLOCK) ON zz.primary_zone_id=tro.zone_id
					AND (zz.start_date<=t.start_date AND (zz.end_date>=t.start_date OR zz.end_date IS NULL))
				JOIN uvw_systemzone_universe sz (NOLOCK) ON sz.zone_id=zz.secondary_zone_id
					AND (sz.start_date<=t.start_date AND (sz.end_date>=t.start_date OR sz.end_date IS NULL))
					AND sz.type='BILLING'
			WHERE
				(tro.start_date <= @end_date AND tro.end_date >= @start_date)
			GROUP BY
				sz.system_id
		
		IF OBJECT_ID('tempdb..#invoiced') IS NOT NULL DROP TABLE #invoiced;
		CREATE TABLE #invoiced (system_id INT)
		INSERT INTO #invoiced
			SELECT
				i.system_id
			FROM
				invoices i (NOLOCK)
			WHERE
				i.media_month_id=@media_month_id
			GROUP BY
				i.system_id

		IF OBJECT_ID('tempdb..#output') IS NOT NULL DROP TABLE #output;
		CREATE TABLE #output (system_id INT)
		INSERT INTO #output
			SELECT system_id FROM #trafficked
			EXCEPT
			SELECT system_id FROM #invoiced
	 
		-- must match "systems" field structure/order
		SELECT
			s.system_id,
			s.code,
			s.name,
			s.location,
			s.spot_yield_weight,
			s.traffic_order_format,
			s.flag,
			s.active,
			s.start_date,
			s.generate_traffic_alert_excel,
			s.one_advertiser_per_traffic_alert,
			s.cancel_recreate_order_traffic_alert,
			s.order_regeneration_traffic_alert,
			s.custom_traffic_system  
		FROM
			#output o
			JOIN uvw_system_universe s (NOLOCK) ON s.system_id=o.system_id
				AND (s.start_date<=@start_date AND (s.end_date>=@start_date OR s.end_date IS NULL))
	END
