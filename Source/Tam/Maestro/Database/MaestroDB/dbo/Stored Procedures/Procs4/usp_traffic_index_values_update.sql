CREATE PROCEDURE usp_traffic_index_values_update
(
	@id		Int,
	@network_id		Int,
	@media_month_id		Int,
	@spot_length_id		Int,
	@index_value		Float
)
AS
UPDATE traffic_index_values SET
	network_id = @network_id,
	media_month_id = @media_month_id,
	spot_length_id = @spot_length_id,
	index_value = @index_value
WHERE
	id = @id

