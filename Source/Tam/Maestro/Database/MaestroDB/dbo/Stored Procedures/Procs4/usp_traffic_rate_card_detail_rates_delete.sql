CREATE PROCEDURE usp_traffic_rate_card_detail_rates_delete
(
	@traffic_rate_card_detail_id		Int,
	@spot_length_id		Int)
AS
DELETE FROM
	traffic_rate_card_detail_rates
WHERE
	traffic_rate_card_detail_id = @traffic_rate_card_detail_id
 AND
	spot_length_id = @spot_length_id
