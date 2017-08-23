CREATE PROCEDURE usp_nielsen_suggested_rate_details_insert
(
	@id		Int		OUTPUT,
	@nielsen_suggested_rate_id		Int,
	@network_id		Int,
	@daypart_id		Int,
	@suggested_cpm		Money,
	@total_spend		Money,
	@total_delivery		Float,
	@network_group_type		TinyInt
)
AS
INSERT INTO nielsen_suggested_rate_details
(
	nielsen_suggested_rate_id,
	network_id,
	daypart_id,
	suggested_cpm,
	total_spend,
	total_delivery,
	network_group_type
)
VALUES
(
	@nielsen_suggested_rate_id,
	@network_id,
	@daypart_id,
	@suggested_cpm,
	@total_spend,
	@total_delivery,
	@network_group_type
)

SELECT
	@id = SCOPE_IDENTITY()

