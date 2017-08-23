CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficIndexTopographyMapSet]

AS

SELECT 
	distinct index_map.topography_id
FROM
	topography_maps index_map(NOLOCK) 
WHERE
	index_map.map_set = 'traffic_index'
