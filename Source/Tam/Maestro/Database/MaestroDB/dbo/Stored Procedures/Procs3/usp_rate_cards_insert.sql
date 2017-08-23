
CREATE PROCEDURE [dbo].[usp_rate_cards_insert]
(
	@id		int		OUTPUT,
	@rate_card_book_id		Int,
	@rate_card_type_id		Int,
	@daypart_id		Int
)
AS
INSERT INTO rate_cards
(
	rate_card_book_id,
	rate_card_type_id,
	daypart_id
)
VALUES
(
	@rate_card_book_id,
	@rate_card_type_id,
	@daypart_id
)

SELECT
	@id = @@Identity
