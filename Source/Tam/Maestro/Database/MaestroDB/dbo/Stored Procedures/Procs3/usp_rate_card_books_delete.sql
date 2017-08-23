CREATE PROCEDURE usp_rate_card_books_delete
(
	@id Int
)
AS
DELETE FROM rate_card_books WHERE id=@id
