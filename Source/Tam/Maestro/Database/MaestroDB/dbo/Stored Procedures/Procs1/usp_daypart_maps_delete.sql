CREATE PROCEDURE usp_daypart_maps_delete
(
	@id Int
)
AS
DELETE FROM daypart_maps WHERE id=@id
