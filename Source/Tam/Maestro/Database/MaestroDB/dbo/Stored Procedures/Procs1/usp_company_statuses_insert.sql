CREATE PROCEDURE usp_company_statuses_insert
(
	@id		Int		OUTPUT,
	@name		VarChar(50),
	@code		VarChar(2)
)
AS
INSERT INTO company_statuses
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

