CREATE PROCEDURE usp_dma_histories_update
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
UPDATE dma_histories SET
	code = @code,
	name = @name,
	rank = @rank,
	tv_hh = @tv_hh,
	cable_hh = @cable_hh,
	active = @active,
	end_date = @end_date,
	flag = @flag
WHERE
	dma_id = @dma_id AND
	start_date = @start_date
