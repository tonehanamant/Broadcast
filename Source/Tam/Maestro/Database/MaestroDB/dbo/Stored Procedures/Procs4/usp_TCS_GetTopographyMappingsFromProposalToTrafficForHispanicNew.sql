
CREATE PROCEDURE [dbo].[usp_TCS_GetTopographyMappingsFromProposalToTrafficForHispanicNew]
(
                @topography_id int
)
AS
                                SELECT
                                                tm.id,
                                                tm.topography_id,
                                                tm.map_set,
                                                tm.map_value,
                                                topographies.code,
                                                CAST(isnull(isopt.map_value, 0) as bit)
                                FROM
                                                topography_maps tm (NOLOCK) join
                                                topographies (NOLOCK) on topographies.id = tm.topography_id
                                                left join topography_maps isopt (NOLOCK) on isopt.topography_id = topographies.id and isopt.map_set = 'topo_opto'
                                WHERE
                                                tm.map_set = 'prpsl_trffc_hisp_n' 
                                                AND CAST(tm.map_value as INT) = @topography_id
                                ORDER BY
                                                tm.id
