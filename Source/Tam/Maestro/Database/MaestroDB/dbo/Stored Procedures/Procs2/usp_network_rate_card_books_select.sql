CREATE PROCEDURE usp_network_rate_card_books_select
(
	@id Int
)
AS
SELECT
	*
FROM
	network_rate_card_books WITH(NOLOCK)
WHERE
	id = @id
