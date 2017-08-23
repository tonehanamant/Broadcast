
-- =============================================
-- Author:		David Chen
-- Create date: 7/5/2016
-- Modified:	7/5/2016
-- Description:	Deletes all daily inventory forecasts and forecast details on and after a certain mediamonthid
-- =============================================
/*
	EXEC usp_ICS_GetReservedProposalInventoryForMediaPlanByProposalId 66708
*/
CREATE PROCEDURE [dbo].[usp_ICS_DeleteForecastAndForecastDetailsOnAndAfterMediaMonthId]
	@media_month_id INT
AS
BEGIN
	DECLARE @partition_number INT
	SELECT TOP 1 @partition_number = $partition.MediaMonthSmallintPFN(media_month_id) FROM dbo.daily_inventory_forecasts (NOLOCK) WHERE media_month_id = @media_month_id
	
	IF @partition_number IS NOT NULL
	BEGIN                
	       ALTER TABLE dbo.daily_inventory_forecasts
	              SWITCH PARTITION @partition_number TO [dbo].daily_inventory_forecasts_trunc PARTITION @partition_number
	
	       TRUNCATE TABLE daily_inventory_forecasts_trunc;
	END

	SELECT TOP 1 @partition_number = $partition.MediaMonthSmallintPFN(media_month_id) FROM dbo.daily_inventory_forecast_details (NOLOCK) WHERE media_month_id = @media_month_id
	
	IF @partition_number IS NOT NULL
	BEGIN                
	       ALTER TABLE dbo.daily_inventory_forecast_details
	              SWITCH PARTITION @partition_number TO [dbo].daily_inventory_forecast_details_trunc PARTITION @partition_number
	
	       TRUNCATE TABLE daily_inventory_forecast_details_trunc;
	END
END