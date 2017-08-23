CREATE PROCEDURE usp_rotational_biases_update
(
	@id		Int,
	@media_month_id		Int,
	@daypart_id		Int,
	@nielsen_network_id		Int,
	@audience_id		Int,
	@spot_length_id		Int,
	@bias		Float
)
AS
UPDATE rotational_biases SET
	media_month_id = @media_month_id,
	daypart_id = @daypart_id,
	nielsen_network_id = @nielsen_network_id,
	audience_id = @audience_id,
	spot_length_id = @spot_length_id,
	bias = @bias
WHERE
	id = @id

