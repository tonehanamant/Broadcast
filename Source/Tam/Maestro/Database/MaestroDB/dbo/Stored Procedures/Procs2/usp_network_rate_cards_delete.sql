CREATE PROCEDURE usp_network_rate_cards_delete
(
	@id Int
)
AS
DELETE FROM network_rate_cards WHERE id=@id
