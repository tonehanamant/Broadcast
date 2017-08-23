CREATE PROCEDURE usp_dmas_update
(
	@id		Int,
	@code		VarChar(15),
	@name		VarChar(63),
	@rank		Int,
	@tv_hh		Int,
	@cable_hh		Int,
	@active		Bit,
	@effective_date		DateTime,
	@flag		TinyInt
)
AS
UPDATE dmas SET
	code = @code,
	name = @name,
	rank = @rank,
	tv_hh = @tv_hh,
	cable_hh = @cable_hh,
	active = @active,
	effective_date = @effective_date,
	flag = @flag
WHERE
	id = @id

