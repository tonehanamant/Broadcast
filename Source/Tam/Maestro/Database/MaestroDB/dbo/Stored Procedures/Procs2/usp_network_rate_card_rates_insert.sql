CREATE PROCEDURE usp_network_rate_card_rates_insert
(
	@id		Int		OUTPUT,
	@network_rate_card_detail_id		Int,
	@spot_length_id		Int,
	@rate		Money
)
AS
INSERT INTO network_rate_card_rates
(
	network_rate_card_detail_id,
	spot_length_id,
	rate
)
VALUES
(
	@network_rate_card_detail_id,
	@spot_length_id,
	@rate
)

SELECT
	@id = SCOPE_IDENTITY()

