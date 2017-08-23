-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/5/2011
-- Description: 
-- =============================================
-- usp_ACS_GetTrafficOrderNetworkLinesByMediaMonth 356
CREATE PROCEDURE [usp_ACS_GetTrafficOrderNetworkLineInfoByMediaMonth]
	@media_month_id INT
AS
BEGIN
	DECLARE @media_month VARCHAR(15)
	SELECT @media_month = media_month FROM media_months (NOLOCK) WHERE id=@media_month_id
	
	SELECT 
		td.id,
		tr.system_id,
		MIN(tr.start_date),
		MAX(tr.end_date) 
	FROM 
		traffic_details td (NOLOCK) 
		JOIN traffic_orders tr (NOLOCK) ON tr.traffic_detail_id=td.id 
	WHERE 
		td.traffic_id IN (
			SELECT traffic_id FROM dbo.udf_ReleasedTraffic(@media_month,0)
	) 
	GROUP BY 
		td.id,
		tr.system_id
END
