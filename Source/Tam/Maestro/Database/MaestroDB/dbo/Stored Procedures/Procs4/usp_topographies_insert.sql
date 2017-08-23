CREATE PROCEDURE usp_topographies_insert
(
	@id		Int		OUTPUT,
	@code		VarChar(15),
	@name		VarChar(63),
	@topography_type		TinyInt
)
AS
INSERT INTO topographies
(
	code,
	name,
	topography_type
)
VALUES
(
	@code,
	@name,
	@topography_type
)

SELECT
	@id = SCOPE_IDENTITY()

