CREATE PROCEDURE [dbo].[usp_traffic_topography_rate_card_map_delete]
(
                @traffic_id                         Int,
                @topography_id                              Int)
AS
DELETE FROM
                traffic_topography_rate_card_map
WHERE
                traffic_id = @traffic_id
AND
                topography_id = @topography_id
