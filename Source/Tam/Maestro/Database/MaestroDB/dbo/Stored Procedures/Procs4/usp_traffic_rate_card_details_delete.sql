CREATE PROCEDURE usp_traffic_rate_card_details_delete
(
	@id Int
)
AS
DELETE FROM traffic_rate_card_details WHERE id=@id
