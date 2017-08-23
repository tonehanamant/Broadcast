CREATE PROCEDURE usp_traffic_index_index_delete
(
	@id Int
)
AS
DELETE FROM traffic_index_index WHERE id=@id
