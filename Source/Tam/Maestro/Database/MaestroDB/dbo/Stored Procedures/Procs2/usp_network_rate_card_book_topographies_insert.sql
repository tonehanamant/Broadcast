CREATE PROCEDURE usp_network_rate_card_book_topographies_insert
(
	@network_rate_card_book_id		Int,
	@topography_id		Int
)
AS
INSERT INTO network_rate_card_book_topographies
(
	network_rate_card_book_id,
	topography_id
)
VALUES
(
	@network_rate_card_book_id,
	@topography_id
)

