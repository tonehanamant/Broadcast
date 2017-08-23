CREATE PROCEDURE usp_system_groups_insert
(
	@id		Int		OUTPUT,
	@name		VarChar(63),
	@active		Bit,
	@flag		TinyInt,
	@effective_date		DateTime
)
AS
INSERT INTO system_groups
(
	name,
	active,
	flag,
	effective_date
)
VALUES
(
	@name,
	@active,
	@flag,
	@effective_date
)

SELECT
	@id = SCOPE_IDENTITY()

