CREATE PROCEDURE usp_network_rate_card_rates_delete
(
	@id Int
)
AS
DELETE FROM network_rate_card_rates WHERE id=@id
