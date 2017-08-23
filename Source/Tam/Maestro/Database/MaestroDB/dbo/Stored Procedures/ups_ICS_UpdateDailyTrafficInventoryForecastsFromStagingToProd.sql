-- =============================================
-- Author:           David Chen
-- Create date: 8/18/2016
-- Modified:
-- Description:      
-- =============================================
/*
       EXEC ups_ICS_UpdateDailyTrafficInventoryForecastsFromStagingToProd 401
*/
CREATE PROCEDURE [dbo].[ups_ICS_UpdateDailyTrafficInventoryForecastsFromStagingToProd]
       @media_month_id INT
AS
BEGIN
	DECLARE @partition_number INT;

	TRUNCATE TABLE daily_traffic_inventory_forecasts_trunc;
    TRUNCATE TABLE daily_traffic_inventory_forecast_details_trunc;
	
	-- daily_traffic_inventory_forecasts: move from PROD to TRUNC
	SELECT TOP 1 @partition_number = $partition.MediaMonthSmallintPFN(media_month_id) FROM dbo.daily_traffic_inventory_forecasts (NOLOCK) WHERE media_month_id = @media_month_id
       
	IF @partition_number IS NOT NULL
	BEGIN                
		ALTER TABLE dbo.daily_traffic_inventory_forecasts
			SWITCH PARTITION @partition_number TO dbo.daily_traffic_inventory_forecasts_trunc PARTITION @partition_number
       
		TRUNCATE TABLE daily_traffic_inventory_forecasts_trunc;
	END

	INSERT INTO dbo.daily_traffic_inventory_forecasts
		SELECT * FROM dbo.daily_traffic_inventory_forecasts_staging WHERE media_month_id=@media_month_id;

	DELETE FROM dbo.daily_traffic_inventory_forecasts_staging WHERE media_month_id=@media_month_id;

	-- daily_traffic_inventory_forecast_details: move from PROD to TRUNC
	SELECT TOP 1 @partition_number = $partition.MediaMonthSmallintPFN(media_month_id) FROM dbo.daily_traffic_inventory_forecast_details (NOLOCK) WHERE media_month_id = @media_month_id
       
	IF @partition_number IS NOT NULL
	BEGIN
		ALTER TABLE dbo.daily_traffic_inventory_forecast_details
			SWITCH PARTITION @partition_number TO dbo.daily_traffic_inventory_forecast_details_trunc PARTITION @partition_number
       
		TRUNCATE TABLE daily_traffic_inventory_forecast_details_trunc;
	END

	INSERT INTO dbo.daily_traffic_inventory_forecast_details
		SELECT * FROM dbo.daily_traffic_inventory_forecast_details_staging WHERE media_month_id=@media_month_id;

	DELETE FROM dbo.daily_traffic_inventory_forecast_details_staging WHERE media_month_id=@media_month_id;

	TRUNCATE TABLE daily_traffic_inventory_forecasts_trunc;
    TRUNCATE TABLE daily_traffic_inventory_forecast_details_trunc;
END