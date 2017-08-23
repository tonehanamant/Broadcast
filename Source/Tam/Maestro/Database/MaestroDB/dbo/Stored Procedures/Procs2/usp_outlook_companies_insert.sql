CREATE PROCEDURE usp_outlook_companies_insert
(
	@id		Int		OUTPUT,
	@name		VarChar(127)
)
AS
INSERT INTO outlook_companies
(
	name
)
VALUES
(
	@name
)

SELECT
	@id = SCOPE_IDENTITY()

