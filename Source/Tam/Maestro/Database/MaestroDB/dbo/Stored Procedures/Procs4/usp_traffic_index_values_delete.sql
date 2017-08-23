CREATE PROCEDURE usp_traffic_index_values_delete
(
	@id Int
)
AS
DELETE FROM traffic_index_values WHERE id=@id
