CREATE PROCEDURE usp_clusters_delete
(
	@id Int
)
AS
DELETE FROM clusters WHERE id=@id
