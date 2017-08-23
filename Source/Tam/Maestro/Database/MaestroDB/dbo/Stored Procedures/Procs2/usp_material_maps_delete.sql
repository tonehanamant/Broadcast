CREATE PROCEDURE usp_material_maps_delete
(
	@id Int
,
	@effective_date DateTime
)
AS
UPDATE material_maps SET active=0, effective_date=@effective_date WHERE id=@id
