CREATE PROCEDURE usp_company_types_select
(
	@id Int
)
AS
SELECT
	*
FROM
	company_types WITH(NOLOCK)
WHERE
	id = @id
