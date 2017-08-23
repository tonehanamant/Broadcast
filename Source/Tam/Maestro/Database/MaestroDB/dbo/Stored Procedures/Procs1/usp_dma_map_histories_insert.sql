CREATE PROCEDURE usp_dma_map_histories_insert
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
INSERT INTO dma_map_histories
(
	dma_map_id,
	start_date,
	dma_id,
	map_set,
	map_value,
	active,
	flag,
	end_date
)
VALUES
(
	@dma_map_id,
	@start_date,
	@dma_id,
	@map_set,
	@map_value,
	@active,
	@flag,
	@end_date
)

