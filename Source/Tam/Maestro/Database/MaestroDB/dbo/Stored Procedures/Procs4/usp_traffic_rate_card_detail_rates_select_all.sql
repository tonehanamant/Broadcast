CREATE PROCEDURE usp_traffic_rate_card_detail_rates_select_all
AS
SELECT
	*
FROM
	traffic_rate_card_detail_rates WITH(NOLOCK)
