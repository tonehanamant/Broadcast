CREATE PROCEDURE usp_employees_select
(
	@id Int
)
AS
SELECT
	*
FROM
	employees WITH(NOLOCK)
WHERE
	id = @id
