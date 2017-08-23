CREATE PROCEDURE usp_expert_biases_update
(
	@id		Int,
	@media_month_id		Int,
	@nielsen_network_id		Int,
	@audience_id		Int,
	@bias		Float
)
AS
UPDATE expert_biases SET
	media_month_id = @media_month_id,
	nielsen_network_id = @nielsen_network_id,
	audience_id = @audience_id,
	bias = @bias
WHERE
	id = @id

