CREATE PROCEDURE usp_network_rate_card_book_topographies_delete
(
	@network_rate_card_book_id		Int,
	@topography_id		Int)
AS
DELETE FROM
	network_rate_card_book_topographies
WHERE
	network_rate_card_book_id = @network_rate_card_book_id
 AND
	topography_id = @topography_id
