CREATE PROCEDURE usp_businesses_insert
(
	@id		Int		OUTPUT,
	@code		VarChar(15),
	@name		VarChar(63),
	@type		VarChar(15),
	@active		Bit,
	@effective_date		DateTime
)
AS
INSERT INTO businesses
(
	code,
	name,
	type,
	active,
	effective_date
)
VALUES
(
	@code,
	@name,
	@type,
	@active,
	@effective_date
)

SELECT
	@id = SCOPE_IDENTITY()

