CREATE PROCEDURE usp_rate_card_books_select_all
AS
SELECT
	*
FROM
	rate_card_books WITH(NOLOCK)
