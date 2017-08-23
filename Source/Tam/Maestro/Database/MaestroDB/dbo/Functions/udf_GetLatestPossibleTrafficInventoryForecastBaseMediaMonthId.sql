﻿-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 8/20/2016
-- Description:	Get latest possible base media month id for an inventory forecast.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetLatestPossibleTrafficInventoryForecastBaseMediaMonthId]
(
	@effective_date DATETIME
)
RETURNS INT
AS
BEGIN	
	DECLARE @return INT;
	DECLARE @media_month_id INT;
	
	SELECT @media_month_id = mm.id FROM media_months mm WHERE @effective_date BETWEEN mm.start_date AND mm.end_date;
	
	SELECT @return = MAX(inv.base_media_month_id) FROM traffic_inventory_forecasts inv WHERE inv.forecast_media_month_id=@media_month_id;
	
	RETURN @return;
END