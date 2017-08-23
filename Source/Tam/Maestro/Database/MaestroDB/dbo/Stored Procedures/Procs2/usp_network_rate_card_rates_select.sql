CREATE PROCEDURE usp_network_rate_card_rates_select
(
	@id Int
)
AS
SELECT
	*
FROM
	network_rate_card_rates WITH(NOLOCK)
WHERE
	id = @id
