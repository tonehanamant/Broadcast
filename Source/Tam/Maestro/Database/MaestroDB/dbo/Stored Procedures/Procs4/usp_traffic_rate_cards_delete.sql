CREATE PROCEDURE usp_traffic_rate_cards_delete
(
	@id Int
)
AS
DELETE FROM traffic_rate_cards WHERE id=@id
