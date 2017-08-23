CREATE PROCEDURE usp_network_rate_card_book_topographies_select
(
	@network_rate_card_book_id		Int,
	@topography_id		Int
)
AS
SELECT
	*
FROM
	network_rate_card_book_topographies WITH(NOLOCK)
WHERE
	network_rate_card_book_id=@network_rate_card_book_id
	AND
	topography_id=@topography_id

