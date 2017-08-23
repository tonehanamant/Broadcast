CREATE PROCEDURE usp_rate_card_books_select
(
	@id Int
)
AS
SELECT
	*
FROM
	rate_card_books WITH(NOLOCK)
WHERE
	id = @id
