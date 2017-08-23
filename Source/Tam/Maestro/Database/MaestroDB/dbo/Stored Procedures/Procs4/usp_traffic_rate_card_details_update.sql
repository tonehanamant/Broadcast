CREATE PROCEDURE usp_traffic_rate_card_details_update
(
	@id		Int,
	@traffic_rate_card_id		Int,
	@network_id		Int,
	@daypart_id		Int
)
AS
UPDATE traffic_rate_card_details SET
	traffic_rate_card_id = @traffic_rate_card_id,
	network_id = @network_id,
	daypart_id = @daypart_id
WHERE
	id = @id

