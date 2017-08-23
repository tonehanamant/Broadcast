CREATE PROCEDURE usp_dma_map_histories_update
(
	@dma_map_id		Int,
	@start_date		DateTime,
	@dma_id		Int,
	@map_set		VarChar(15),
	@map_value		VarChar(63),
	@active		Bit,
	@flag		TinyInt,
	@end_date		DateTime
)
AS
UPDATE dma_map_histories SET
	dma_id = @dma_id,
	map_set = @map_set,
	map_value = @map_value,
	active = @active,
	flag = @flag,
	end_date = @end_date
WHERE
	dma_map_id = @dma_map_id AND
	start_date = @start_date
