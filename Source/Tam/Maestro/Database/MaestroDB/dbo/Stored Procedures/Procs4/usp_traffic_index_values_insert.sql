CREATE PROCEDURE usp_traffic_index_values_insert
(
	@id		Int		OUTPUT,
	@network_id		Int,
	@media_month_id		Int,
	@spot_length_id		Int,
	@index_value		Float
)
AS
INSERT INTO traffic_index_values
(
	network_id,
	media_month_id,
	spot_length_id,
	index_value
)
VALUES
(
	@network_id,
	@media_month_id,
	@spot_length_id,
	@index_value
)

SELECT
	@id = SCOPE_IDENTITY()

