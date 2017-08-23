CREATE PROCEDURE usp_dmas_insert
(
	@id		Int		OUTPUT,
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
INSERT INTO dmas
(
	code,
	name,
	rank,
	tv_hh,
	cable_hh,
	active,
	effective_date,
	flag
)
VALUES
(
	@code,
	@name,
	@rank,
	@tv_hh,
	@cable_hh,
	@active,
	@effective_date,
	@flag
)

SELECT
	@id = SCOPE_IDENTITY()

