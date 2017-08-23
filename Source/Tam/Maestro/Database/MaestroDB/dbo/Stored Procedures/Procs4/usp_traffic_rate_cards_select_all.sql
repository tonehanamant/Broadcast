CREATE PROCEDURE usp_traffic_rate_cards_select_all
AS
SELECT
	*
FROM
	traffic_rate_cards WITH(NOLOCK)
