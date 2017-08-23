CREATE PROCEDURE usp_dma_maps_delete
(
	@id Int
,
	@effective_date DateTime
)
AS
UPDATE dma_maps SET active=0, effective_date=@effective_date WHERE id=@id
