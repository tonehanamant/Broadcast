-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/5/2011
-- Description: 
-- =============================================
-- usp_ACS_GetTrafficOrderInfoByMediaMonth 356
CREATE PROCEDURE [dbo].[usp_ACS_GetTrafficOrderInfoByMediaMonth]
	@media_month_id INT
AS
BEGIN
	DECLARE @media_month VARCHAR(15)
	SELECT @media_month = media_month FROM media_months (NOLOCK) WHERE id=@media_month_id

	SELECT 
		CAST(tr.id AS BIGINT),
		td.traffic_id,
		td.id,
		tr.system_id,
		tr.daypart_id,
		tr.start_date,
		tr.end_date,
		d.mon,
		d.tue,
		d.wed,
		d.thu,
		d.fri,
		d.sat,
		d.sun,
		d.start_time,
		d.end_time,
		tr.ordered_spot_rate,
		tr.subscribers,
		tr.zone_id 
	FROM 
		traffic_orders tr (NOLOCK) 
		JOIN traffic_details td (NOLOCK) ON tr.traffic_detail_id=td.id 
		JOIN vw_ccc_daypart d (NOLOCK) ON d.id=tr.daypart_id 
	WHERE 
		tr.on_financial_reports=1 
		AND tr.active=1 
		AND ((tr.ordered_spots=0 AND tr.system_id IN (667,668)) OR tr.ordered_spots>0) 
		AND td.traffic_id IN (
			SELECT traffic_id FROM dbo.udf_ReleasedTraffic(@media_month,0)
		)
END
