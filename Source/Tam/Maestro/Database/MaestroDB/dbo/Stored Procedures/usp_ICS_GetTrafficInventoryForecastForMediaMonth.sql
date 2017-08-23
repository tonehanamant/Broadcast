-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/16/2015
-- Description:	Returns lastest traffic inventory forecast specific to the dimensions of network and media month.
-- =============================================
/*
	DECLARE @media_month_id SMALLINT = 408					
	EXEC usp_ICS_GetTrafficInventoryForecastForMediaMonth @media_month_id
*/
CREATE PROCEDURE [dbo].[usp_ICS_GetTrafficInventoryForecastForMediaMonth]
	@media_month_id INT
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
		
	DECLARE @start_date DATETIME;
	SELECT @start_date = MIN(mm.start_date) FROM media_months mm WHERE mm.id=@media_month_id;
	
	DECLARE @latest_base_media_month_id SMALLINT;
	SELECT @latest_base_media_month_id = dbo.udf_GetLatestPossibleTrafficInventoryForecastBaseMediaMonthId(@start_date);

	SELECT mm.* FROM media_months mm WHERE mm.id=@latest_base_media_month_id;

	SELECT
		inv.forecast_media_month_id 'ForecastMediaMonthId',
		inv.forecast_media_week_id 'ForecastMediaWeekId',
		inv.network_id 'NetworkId',
		inv.business_id 'BusinessId',
		inv.component_daypart_id 'ComponentDaypartId',
		inv.hh_eq_cpm_start 'HhEqCpmStart',
		inv.hh_eq_cpm_end 'HhEqCpmEnd',
		SUM(inv.subscribers) 'Subscribers'
	FROM
		dbo.traffic_inventory_forecasts inv
	WHERE
		inv.base_media_month_id=@latest_base_media_month_id
		AND inv.forecast_media_month_id=@media_month_id
	GROUP BY
		inv.forecast_media_month_id,
		inv.forecast_media_week_id,
		inv.network_id,
		inv.business_id,
		inv.component_daypart_id,
		inv.hh_eq_cpm_start,
		inv.hh_eq_cpm_end
END