-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/5/2011
-- Description: 
-- =============================================
-- usp_ACS_GetTrafficDetailWeekInfoByMediaMonth 356
CREATE PROCEDURE usp_ACS_GetTrafficDetailWeekInfoByMediaMonth
	@media_month_id INT
AS
BEGIN
	DECLARE @media_month VARCHAR(15)
	SELECT @media_month = media_month FROM media_months (NOLOCK) WHERE id=@media_month_id
	
	SELECT 
		td.traffic_id,
		td.id,
		tdw.start_date,
		tdw.end_date,
		tdw.suspended 
	FROM 
		traffic_details td (NOLOCK) 
		JOIN traffic_detail_weeks tdw (NOLOCK) ON tdw.traffic_detail_id=td.id
	WHERE
		td.traffic_id IN (
			SELECT traffic_id FROM dbo.udf_ReleasedTraffic(@media_month,0)
		) 
END
