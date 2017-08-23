CREATE PROCEDURE usp_traffic_materials_disposition_delete
(
	@id Int
)
AS
DELETE FROM traffic_materials_disposition WHERE id=@id
