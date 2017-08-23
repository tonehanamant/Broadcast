CREATE PROCEDURE usp_network_rate_cards_select
(
	@id Int
)
AS
SELECT
	*
FROM
	network_rate_cards WITH(NOLOCK)
WHERE
	id = @id
