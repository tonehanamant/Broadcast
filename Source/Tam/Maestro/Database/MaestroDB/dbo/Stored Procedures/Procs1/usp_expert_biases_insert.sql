CREATE PROCEDURE usp_expert_biases_insert
(
	@id		Int		OUTPUT,
	@media_month_id		Int,
	@nielsen_network_id		Int,
	@audience_id		Int,
	@bias		Float
)
AS
INSERT INTO expert_biases
(
	media_month_id,
	nielsen_network_id,
	audience_id,
	bias
)
VALUES
(
	@media_month_id,
	@nielsen_network_id,
	@audience_id,
	@bias
)

SELECT
	@id = SCOPE_IDENTITY()

