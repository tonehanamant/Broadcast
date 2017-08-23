CREATE PROCEDURE usp_traffic_rate_card_detail_rates_select
(
	@traffic_rate_card_detail_id		Int,
	@spot_length_id		Int
)
AS
SELECT
	*
FROM
	traffic_rate_card_detail_rates WITH(NOLOCK)
WHERE
	traffic_rate_card_detail_id=@traffic_rate_card_detail_id
	AND
	spot_length_id=@spot_length_id

