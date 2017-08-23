CREATE PROCEDURE usp_network_rate_card_books_delete
(
	@id Int
)
AS
DELETE FROM network_rate_card_books WHERE id=@id
