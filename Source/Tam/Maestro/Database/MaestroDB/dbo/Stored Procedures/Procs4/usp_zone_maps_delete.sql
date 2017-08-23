CREATE PROCEDURE usp_zone_maps_delete
(
	@id Int
,
	@effective_date DateTime
)
AS
UPDATE zone_maps SET active=0, effective_date=@effective_date WHERE id=@id
