CREATE PROCEDURE usp_network_rate_card_rates_select_all
AS
SELECT
	*
FROM
	network_rate_card_rates WITH(NOLOCK)
