CREATE PROCEDURE usp_company_statuses_select
(
	@id Int
)
AS
SELECT
	*
FROM
	company_statuses WITH(NOLOCK)
WHERE
	id = @id
