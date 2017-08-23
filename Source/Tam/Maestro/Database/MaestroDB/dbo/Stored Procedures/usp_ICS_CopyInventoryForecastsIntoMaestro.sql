-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/19/2015
-- Description:	This procedure copies the Sales and Traffic inventory forecasts from the inventory database into the maestro database.
--				Data flows from inventory.dbo.sales_inventory_forecasts to maestro.dbo.inventory_forecasts and inventory.dbo.traffic_inventory_forecasts to maestro.dbo.traffic_inventory_forecasts
-- =============================================
CREATE PROCEDURE [dbo].[usp_ICS_CopyInventoryForecastsIntoMaestro]
	@base_media_month_id INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @cpm_bin_size MONEY = 0.25;
	DECLARE @base_media_month VARCHAR(4);
	SELECT @base_media_month = mm.media_month FROM media_months mm WHERE mm.id=@base_media_month_id;

	-- (1) handle sales forecast
	-- (2) handle traffic forecast

	-- if data already exists for month clear it
	IF (SELECT COUNT(1) FROM dbo.inventory_forecasts inv WHERE inv.base_media_month_id=@base_media_month_id) > 0
	BEGIN
		DELETE FROM dbo.inventory_forecasts WHERE base_media_month_id=@base_media_month_id;
	END
	IF (SELECT COUNT(1) FROM dbo.traffic_inventory_forecasts inv WHERE inv.base_media_month_id=@base_media_month_id) > 0
	BEGIN
		DELETE FROM dbo.traffic_inventory_forecasts WHERE base_media_month_id=@base_media_month_id;
	END

	-- stage inventory dayparts for fast JOIN to predicted inventory structure
	DECLARE @component_dayparts TABLE (week_days BIT NOT NULL, week_ends BIT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, daypart_id INT NOT NULL, PRIMARY KEY CLUSTERED(week_days,week_ends,start_time,end_time));
	INSERT INTO @component_dayparts
		SELECT 
			(cd.mon | cd.tue | cd.wed | cd.thu | cd.fri),
			(cd.sat | cd.sun),
			cd.start_time,
			cd.end_time,
			cd.id
		FROM 
			dbo.GetInventoryComponentDayparts() cd;

	-- (1) handle sales forecast
	INSERT INTO dbo.inventory_forecasts (base_media_month_id, forecast_media_month_id, forecast_media_week_id, network_id, component_daypart_id, hh_eq_cpm_start, hh_eq_cpm_end, subscribers)
		SELECT
			mm_b.id 'base_media_month_id',
			mw.media_month_id 'forecast_media_month_id',
			mw.id 'forecast_media_week_id',
			sif.network_id,
			cd.daypart_id 'component_daypart_id',
			sif.cpm,
			sif.cpm + @cpm_bin_size,
			SUM(sif.subscribers)
		FROM
			inventory.dbo.sales_inventory_forecasts  sif
			JOIN media_months mm_b ON mm_b.media_month=sif.base_media_month
			JOIN media_months mm_f ON mm_f.media_month=sif.forecast_media_month
			JOIN media_weeks mw ON mw.media_month_id=mm_f.id 
				AND mw.week_number=sif.week_number
			JOIN @component_dayparts cd ON sif.seconds BETWEEN cd.start_time AND cd.end_time
				AND 1 =	CASE sif.week_part
							WHEN 0 THEN cd.week_days -- weekdays
							WHEN 1 THEN cd.week_ends -- weekends
						END
		WHERE
			sif.base_media_month=@base_media_month
		GROUP BY
			mm_b.id,
			mw.media_month_id,
			mw.id,
			sif.network_id,
			cd.daypart_id,
			sif.cpm,
			sif.cpm + @cpm_bin_size;
	
	-- (2) handle traffic forecast
	INSERT INTO dbo.traffic_inventory_forecasts (base_media_month_id, forecast_media_month_id, forecast_media_week_id, business_id, network_id, component_daypart_id, hh_eq_cpm_start, hh_eq_cpm_end, subscribers)
		SELECT
			mm_b.id 'base_media_month_id',
			mw.media_month_id 'forecast_media_month_id',
			mw.id 'forecast_media_week_id',
			tif.business_id,
			tif.network_id,
			cd.daypart_id 'component_daypart_id',
			tif.cpm,
			tif.cpm + @cpm_bin_size,
			SUM(tif.subscribers)
		FROM
			inventory.dbo.traffic_inventory_forecasts  tif
			JOIN media_months mm_b ON mm_b.media_month=tif.base_media_month
			JOIN media_months mm_f ON mm_f.media_month=tif.forecast_media_month
			JOIN media_weeks mw ON mw.media_month_id=mm_f.id 
				AND mw.week_number=tif.week_number
			JOIN @component_dayparts cd ON tif.seconds BETWEEN cd.start_time AND cd.end_time
				AND 1 =	CASE tif.week_part
							WHEN 0 THEN cd.week_days -- weekdays
							WHEN 1 THEN cd.week_ends -- weekends
						END
		WHERE
			tif.base_media_month=@base_media_month
		GROUP BY
			mm_b.id,
			mw.media_month_id,
			mw.id,
			tif.business_id,
			tif.network_id,
			cd.daypart_id,
			tif.cpm,
			tif.cpm + @cpm_bin_size;
END