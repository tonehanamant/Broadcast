CREATE PROCEDURE usp_network_rate_cards_insert
(
	@id		Int		OUTPUT,
	@network_rate_card_book_id		Int,
	@rate_card_type_id		Int,
	@daypart_id		Int
)
AS
INSERT INTO network_rate_cards
(
	network_rate_card_book_id,
	rate_card_type_id,
	daypart_id
)
VALUES
(
	@network_rate_card_book_id,
	@rate_card_type_id,
	@daypart_id
)

SELECT
	@id = SCOPE_IDENTITY()

