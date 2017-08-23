CREATE PROCEDURE usp_network_rate_card_rates_update
(
	@id		Int,
	@network_rate_card_detail_id		Int,
	@spot_length_id		Int,
	@rate		Money
)
AS
UPDATE network_rate_card_rates SET
	network_rate_card_detail_id = @network_rate_card_detail_id,
	spot_length_id = @spot_length_id,
	rate = @rate
WHERE
	id = @id

