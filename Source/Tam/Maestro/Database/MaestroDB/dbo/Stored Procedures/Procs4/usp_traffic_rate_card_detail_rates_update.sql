CREATE PROCEDURE usp_traffic_rate_card_detail_rates_update
(
	@traffic_rate_card_detail_id		Int,
	@spot_length_id		Int,
	@rate		Money
)
AS
UPDATE traffic_rate_card_detail_rates SET
	rate = @rate
WHERE
	traffic_rate_card_detail_id = @traffic_rate_card_detail_id AND
	spot_length_id = @spot_length_id
