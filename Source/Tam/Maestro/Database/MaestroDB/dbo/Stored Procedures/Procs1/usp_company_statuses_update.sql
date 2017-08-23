CREATE PROCEDURE usp_company_statuses_update
(
	@id		Int,
	@name		VarChar(50),
	@code		VarChar(2)
)
AS
UPDATE company_statuses SET
	name = @name,
	code = @code
WHERE
	id = @id

