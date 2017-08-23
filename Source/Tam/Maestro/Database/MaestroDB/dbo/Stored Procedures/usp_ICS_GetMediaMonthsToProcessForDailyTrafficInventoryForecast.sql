-- =============================================
-- Author:      Stephen DeFusco
-- Create date: 8/20/2016
-- Description: Get's a list of media months that need to be processed to populate daily_traffic_inventory_forecasts and daily_traffic_inventory_forecast_details.
-- =============================================
-- EXEC usp_ICS_GetMediaMonthsToProcessForDailyTrafficInventoryForecast
CREATE PROCEDURE [dbo].[usp_ICS_GetMediaMonthsToProcessForDailyTrafficInventoryForecast]
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @media_month_ids TABLE (id INT);
	INSERT INTO @media_month_ids
		SELECT 
			mm.id
		FROM 
			proposals p (NOLOCK)
			JOIN media_months mm ON mm.end_date>=p.start_date AND mm.start_date<=p.end_date
			LEFT JOIN daily_inventory_forecast_summaries difs ON difs.media_month_id=mm.id
		WHERE 
			p.include_on_availability_planner=1 
			AND mm.id>=377
		GROUP BY 
			mm.id,
			difs.date_last_started
		HAVING
			difs.date_last_started IS NULL OR MAX(p.date_last_modified)>difs.date_last_started

		UNION

		SELECT 
			mm.id
		FROM 
			traffic t (NOLOCK)
			JOIN media_months mm ON mm.end_date>=t.start_date AND mm.start_date<=t.end_date
			LEFT JOIN daily_traffic_inventory_forecast_summaries difs ON difs.media_month_id=mm.id
		WHERE 
			mm.id>=377
		GROUP BY 
			mm.id,
			difs.date_last_started
		HAVING
			difs.date_last_started IS NULL OR MAX(t.date_last_modified)>difs.date_last_started;

		SELECT
			mm.*
		FROM
			media_months mm
			JOIN @media_month_ids mmi ON mmi.id=mm.id
		ORDER BY
			mm.start_date DESC;
END