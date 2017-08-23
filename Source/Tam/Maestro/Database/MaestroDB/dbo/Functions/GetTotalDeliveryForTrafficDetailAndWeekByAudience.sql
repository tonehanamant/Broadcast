CREATE FUNCTION [dbo].[GetTotalDeliveryForTrafficDetailAndWeekByAudience]
(
	@traffic_detail_id INT,
	@audience_id INT
)
RETURNS @delivery TABLE
(
	traffic_detail_id INT,
	topography_id INT,
	traffic_detail_week_id INT,
	delivery FLOAT
)
AS
BEGIN
	INSERT INTO @delivery (traffic_detail_id, topography_id, traffic_detail_week_id, delivery)
		SELECT 
			td.id, 
			tdt.topography_id, 
			tdw.id, 
			((tda.traffic_rating * dbo.GetTrafficDetailCoverageUniverse(@traffic_detail_id, @audience_id, tdt.topography_id)) / 1000.0) * SUM(tdt.spots) 
		FROM
			traffic_details td (NOLOCK) 
			JOIN traffic_detail_weeks tdw (NOLOCK) ON td.id = tdw.traffic_detail_id
			JOIN traffic_detail_topographies tdt (NOLOCK) ON tdt.traffic_detail_week_id = tdw.id
			JOIN traffic_detail_audiences tda (NOLOCK) ON tda.traffic_detail_id = td.id
		WHERE
			td.id = @traffic_detail_id
			AND tda.audience_id = @audience_id
		GROUP BY
			td.id, 
			tdt.topography_id, 
			tdw.id, 
			tda.traffic_rating;
		
		RETURN;
END

