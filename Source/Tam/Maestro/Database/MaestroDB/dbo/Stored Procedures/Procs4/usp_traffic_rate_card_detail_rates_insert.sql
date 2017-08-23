CREATE PROCEDURE usp_traffic_rate_card_detail_rates_insert
(
	@traffic_rate_card_detail_id		Int,
	@spot_length_id		Int,
	@rate		Money
)
AS
INSERT INTO traffic_rate_card_detail_rates
(
	traffic_rate_card_detail_id,
	spot_length_id,
	rate
)
VALUES
(
	@traffic_rate_card_detail_id,
	@spot_length_id,
	@rate
)

