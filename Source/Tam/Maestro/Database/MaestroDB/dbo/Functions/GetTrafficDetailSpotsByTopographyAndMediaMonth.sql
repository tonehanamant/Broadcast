
CREATE FUNCTION [dbo].[GetTrafficDetailSpotsByTopographyAndMediaMonth]
(
	@traffic_detail_id INT,
	@media_month_id INT,
	@topography_id INT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @return AS FLOAT

	SET @return = (
		SELECT 
			SUM(spots) 
		FROM 
			traffic_detail_topographies tdt (NOLOCK)
			JOIN traffic_detail_weeks tdw (NOLOCK) on tdw.id = tdt.traffic_detail_week_id
			JOIN traffic_details td			(NOLOCK) ON td.id=tdw.traffic_detail_id
			JOIN media_months mm			(NOLOCK) ON
				(mm.start_date <= tdw.end_date AND mm.end_date >= tdw.start_date)
		WHERE 
			tdt.topography_id=@topography_id 
			AND mm.id=@media_month_id
			AND tdw.traffic_detail_id=@traffic_detail_id
			AND 
			(
				tdw.suspended = 0
			)
	)

	RETURN @return
END

