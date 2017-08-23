CREATE PROCEDURE usp_dma_maps_update
(
	@id		Int,
	@dma_id		Int,
	@map_set		VarChar(15),
	@map_value		VarChar(63),
	@active		Bit,
	@flag		TinyInt,
	@effective_date		DateTime
)
AS
UPDATE dma_maps SET
	dma_id = @dma_id,
	map_set = @map_set,
	map_value = @map_value,
	active = @active,
	flag = @flag,
	effective_date = @effective_date
WHERE
	id = @id

