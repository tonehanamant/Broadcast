CREATE PROCEDURE usp_traffic_rate_cards_select
(
	@id Int
)
AS
SELECT
	*
FROM
	traffic_rate_cards WITH(NOLOCK)
WHERE
	id = @id
