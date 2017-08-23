CREATE PROCEDURE usp_dma_histories_insert
(
	@dma_id		Int,
	@start_date		DateTime,
	@code		VarChar(15),
	@name		VarChar(63),
	@rank		Int,
	@tv_hh		Int,
	@cable_hh		Int,
	@active		Bit,
	@end_date		DateTime,
	@flag		TinyInt
)
AS
INSERT INTO dma_histories
(
	dma_id,
	start_date,
	code,
	name,
	rank,
	tv_hh,
	cable_hh,
	active,
	end_date,
	flag
)
VALUES
(
	@dma_id,
	@start_date,
	@code,
	@name,
	@rank,
	@tv_hh,
	@cable_hh,
	@active,
	@end_date,
	@flag
)

