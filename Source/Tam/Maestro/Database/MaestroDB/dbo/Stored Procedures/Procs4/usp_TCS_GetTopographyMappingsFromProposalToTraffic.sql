
CREATE PROCEDURE [dbo].[usp_TCS_GetTopographyMappingsFromProposalToTraffic]
	@topography_id int
AS
BEGIN
	SELECT
		tm.id,
		tm.topography_id,
		tm.map_set,
		tm.map_value,
		topographies.code,
		CAST(isnull(isopt.map_value, 0) as bit)
	FROM
		topography_maps tm (NOLOCK) 
		JOIN topographies (NOLOCK) on topographies.id = tm.topography_id
		LEFT JOIN topography_maps isopt (NOLOCK) on isopt.topography_id = topographies.id and isopt.map_set = 'topo_opto'
	WHERE
		tm.map_set = 'proposal_traffic' 
		AND CAST(tm.map_value as INT) = @topography_id
	ORDER BY
		tm.id
END