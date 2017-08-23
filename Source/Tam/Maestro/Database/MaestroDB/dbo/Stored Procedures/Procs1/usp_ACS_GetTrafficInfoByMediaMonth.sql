-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/5/2011
-- Description: 
-- =============================================
-- usp_ACS_GetTrafficInfoByMediaMonth 356
CREATE PROCEDURE usp_ACS_GetTrafficInfoByMediaMonth
	@media_month_id INT
AS
BEGIN
	DECLARE @media_month VARCHAR(15)
	SELECT @media_month = media_month FROM media_months (NOLOCK) WHERE id=@media_month_id
	
	SELECT 
		td.traffic_id,
		MIN(tr.start_date),
		MAX(tr.end_date) 
	FROM 
		traffic_orders tr (NOLOCK) 
		JOIN traffic_details td (NOLOCK) ON tr.traffic_detail_id=td.id 
	WHERE 
		tr.on_financial_reports=1 
		AND tr.active=1 
		AND ((tr.ordered_spots=0 AND tr.system_id IN (667,668)) OR tr.ordered_spots>0)
		AND td.traffic_id IN (
			SELECT traffic_id FROM dbo.udf_ReleasedTraffic(@media_month,0)
		) 
	GROUP BY 
		td.traffic_id
END
