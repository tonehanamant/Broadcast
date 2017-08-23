CREATE PROCEDURE usp_network_maps_delete
(
	@id Int
,
	@effective_date DateTime
)
AS
UPDATE network_maps SET active=0, effective_date=@effective_date WHERE id=@id
