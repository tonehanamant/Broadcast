CREATE PROCEDURE usp_network_rate_card_books_select_all
AS
SELECT
	*
FROM
	network_rate_card_books WITH(NOLOCK)
