CREATE PROCEDURE [dbo].[usp_traffic_topography_rate_card_map_update]
(
                @traffic_id                         Int,
                @topography_id                              Int,
                @traffic_rate_card_id                   Int
)
AS
UPDATE traffic_topography_rate_card_map SET
                traffic_rate_card_id = @traffic_rate_card_id
WHERE
                traffic_id = @traffic_id AND
                topography_id = @topography_id
