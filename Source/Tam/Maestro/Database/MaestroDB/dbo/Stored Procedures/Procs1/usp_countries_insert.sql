CREATE PROCEDURE usp_countries_insert
(
	@id		Int		OUTPUT,
	@code		VarChar(15),
	@name		VarChar(127)
)
AS
INSERT INTO countries
(
	code,
	name
)
VALUES
(
	@code,
	@name
)

SELECT
	@id = SCOPE_IDENTITY()

