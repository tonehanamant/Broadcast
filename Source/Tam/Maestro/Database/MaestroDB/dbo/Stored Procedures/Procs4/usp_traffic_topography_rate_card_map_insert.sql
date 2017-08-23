CREATE PROCEDURE [dbo].[usp_traffic_topography_rate_card_map_insert]
(
                @traffic_id                         Int,
                @topography_id                              Int,
                @traffic_rate_card_id                   Int
)
AS
INSERT INTO traffic_topography_rate_card_map
(
                traffic_id,
                topography_id,
                traffic_rate_card_id
)
VALUES
(
                @traffic_id,
                @topography_id,
                @traffic_rate_card_id
)
