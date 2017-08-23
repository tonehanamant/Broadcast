CREATE PROCEDURE usp_nielsen_suggested_rate_details_update
(
	@id		Int,
	@nielsen_suggested_rate_id		Int,
	@network_id		Int,
	@daypart_id		Int,
	@suggested_cpm		Money,
	@total_spend		Money,
	@total_delivery		Float,
	@network_group_type		TinyInt
)
AS
UPDATE nielsen_suggested_rate_details SET
	nielsen_suggested_rate_id = @nielsen_suggested_rate_id,
	network_id = @network_id,
	daypart_id = @daypart_id,
	suggested_cpm = @suggested_cpm,
	total_spend = @total_spend,
	total_delivery = @total_delivery,
	network_group_type = @network_group_type
WHERE
	id = @id

