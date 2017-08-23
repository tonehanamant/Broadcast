CREATE PROCEDURE usp_dma_maps_insert
(
	@id		Int		OUTPUT,
	@dma_id		Int,
	@map_set		VarChar(15),
	@map_value		VarChar(63),
	@active		Bit,
	@flag		TinyInt,
	@effective_date		DateTime
)
AS
INSERT INTO dma_maps
(
	dma_id,
	map_set,
	map_value,
	active,
	flag,
	effective_date
)
VALUES
(
	@dma_id,
	@map_set,
	@map_value,
	@active,
	@flag,
	@effective_date
)

SELECT
	@id = SCOPE_IDENTITY()

