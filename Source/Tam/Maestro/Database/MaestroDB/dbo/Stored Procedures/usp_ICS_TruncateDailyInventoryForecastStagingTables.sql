
-- =============================================
-- Author:           Stephen DeFusco
-- Create date: 7/7/2016
-- Description:      <Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ICS_TruncateDailyInventoryForecastStagingTables]
AS
	BEGIN
		   TRUNCATE TABLE dbo.daily_inventory_forecasts_staging;
		   TRUNCATE TABLE dbo.daily_inventory_forecast_details_staging;
	END