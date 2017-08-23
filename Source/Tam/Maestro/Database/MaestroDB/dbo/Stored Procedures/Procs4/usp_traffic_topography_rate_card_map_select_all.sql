CREATE PROCEDURE [dbo].[usp_traffic_topography_rate_card_map_select_all]
AS
SELECT
                *
FROM
                traffic_topography_rate_card_map WITH(NOLOCK)
