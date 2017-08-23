CREATE PROCEDURE usp_network_rate_cards_select_all
AS
SELECT
	*
FROM
	network_rate_cards WITH(NOLOCK)
