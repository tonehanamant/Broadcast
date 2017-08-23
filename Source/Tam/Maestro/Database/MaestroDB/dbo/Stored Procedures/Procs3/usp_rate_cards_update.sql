
CREATE PROCEDURE [dbo].[usp_rate_cards_update]
(
	@id		Int,
	@rate_card_book_id		Int,
	@rate_card_type_id		Int,
	@daypart_id		Int
)
AS
UPDATE rate_cards SET
	rate_card_book_id = @rate_card_book_id,
	rate_card_type_id = @rate_card_type_id,
	daypart_id = @daypart_id
WHERE
	id = @id
