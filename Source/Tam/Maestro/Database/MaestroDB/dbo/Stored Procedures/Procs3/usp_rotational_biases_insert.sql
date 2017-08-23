CREATE PROCEDURE usp_rotational_biases_insert
(
	@id		Int		OUTPUT,
	@media_month_id		Int,
	@daypart_id		Int,
	@nielsen_network_id		Int,
	@audience_id		Int,
	@spot_length_id		Int,
	@bias		Float
)
AS
INSERT INTO rotational_biases
(
	media_month_id,
	daypart_id,
	nielsen_network_id,
	audience_id,
	spot_length_id,
	bias
)
VALUES
(
	@media_month_id,
	@daypart_id,
	@nielsen_network_id,
	@audience_id,
	@spot_length_id,
	@bias
)

SELECT
	@id = SCOPE_IDENTITY()

