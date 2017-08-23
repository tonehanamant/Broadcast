CREATE PROCEDURE usp_company_types_insert
(
	@id		Int		OUTPUT,
	@name		VarChar(20),
	@code		VarChar(3)
)
AS
INSERT INTO company_types
(
	name,
	code
)
VALUES
(
	@name,
	@code
)

SELECT
	@id = SCOPE_IDENTITY()

