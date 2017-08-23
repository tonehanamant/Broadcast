-- =============================================
-- Author:           David Chen
-- Create date: 8/18/2016
-- Description:      <Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ICS_TruncateTrafficDailyTrafficInventoryForecastStagingTables]
AS
	BEGIN
		   TRUNCATE TABLE dbo.daily_traffic_inventory_forecasts_staging;
		   TRUNCATE TABLE dbo.daily_traffic_inventory_forecast_details_staging;
	END