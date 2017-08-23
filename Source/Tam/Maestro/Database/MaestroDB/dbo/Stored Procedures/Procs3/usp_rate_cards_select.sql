
CREATE PROCEDURE [dbo].[usp_rate_cards_select]
(
	@id Int
)
AS
SELECT
	id,
	rate_card_book_id,
	rate_card_type_id,
	daypart_id
FROM
	rate_cards
WHERE
	id = @id
