CREATE PROCEDURE usp_audience_maps_delete
(
	@id Int
)
AS
DELETE FROM audience_maps WHERE id=@id
