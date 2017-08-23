CREATE PROCEDURE [dbo].[usp_traffic_topography_rate_card_map_select]
(
                @traffic_id                         Int,
                @topography_id                              Int
)
AS
SELECT
                *
FROM
                traffic_topography_rate_card_map WITH(NOLOCK)
WHERE
                traffic_id=@traffic_id
                AND
                topography_id=@topography_id
