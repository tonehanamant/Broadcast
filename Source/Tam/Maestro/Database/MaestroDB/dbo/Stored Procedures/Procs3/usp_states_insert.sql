CREATE PROCEDURE usp_states_insert
(
	@id		Int		OUTPUT,
	@code		VarChar(15),
	@name		VarChar(63),
	@active		Bit,
	@flag		TinyInt,
	@effective_date		DateTime
)
AS
INSERT INTO states
(
	code,
	name,
	active,
	flag,
	effective_date
)
VALUES
(
	@code,
	@name,
	@active,
	@flag,
	@effective_date
)

SELECT
	@id = SCOPE_IDENTITY()

