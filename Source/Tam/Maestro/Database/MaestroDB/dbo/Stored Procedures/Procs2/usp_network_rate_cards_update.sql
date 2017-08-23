CREATE PROCEDURE usp_network_rate_cards_update
(
	@id		Int,
	@network_rate_card_book_id		Int,
	@rate_card_type_id		Int,
	@daypart_id		Int
)
AS
UPDATE network_rate_cards SET
	network_rate_card_book_id = @network_rate_card_book_id,
	rate_card_type_id = @rate_card_type_id,
	daypart_id = @daypart_id
WHERE
	id = @id

