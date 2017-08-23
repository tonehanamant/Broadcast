
CREATE FUNCTION [dbo].[GetCustomTrafficFactorForTopography]
(
	@traffic_detail_id INT,
	@topography_id INT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @universe FLOAT
	SET @universe = 0.0

	SELECT DISTINCT @universe = traffic_detail_topographies.universe FROM
		traffic_detail_topographies (NOLOCK) 
		join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.id = traffic_detail_topographies.traffic_detail_week_id
	WHERE
		traffic_detail_weeks.traffic_detail_id = @traffic_detail_id 
		and  traffic_detail_topographies.topography_id = 32

	DECLARE @return FLOAT
		IF @universe > 0.0  		
			BEGIN
			SELECT 
				@return = CAST(topography_maps.map_value as FLOAT) 
			FROM 
				topography_maps (NOLOCK)
			WHERE 
				topography_maps.topography_id = @topography_id 
				and topography_maps.map_set = 'ctf_nfc'
			END
		ELSE
			BEGIN
			SELECT 
				@return = CAST(topography_maps.map_value as FLOAT) 
			FROM 
				topography_maps (NOLOCK)
			WHERE 
				topography_maps.topography_id = @topography_id 
				and topography_maps.map_set = 'ctf_nonfc'
			END
		

	RETURN @return;
END

