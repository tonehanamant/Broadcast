CREATE PROCEDURE usp_network_rate_card_book_topographies_select_all
AS
SELECT
	*
FROM
	network_rate_card_book_topographies WITH(NOLOCK)
