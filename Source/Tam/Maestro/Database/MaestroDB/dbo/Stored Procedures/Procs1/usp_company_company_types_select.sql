CREATE PROCEDURE usp_company_company_types_select
(
	@company_id		Int,
	@company_type_id		Int
)
AS
SELECT
	*
FROM
	company_company_types WITH(NOLOCK)
WHERE
	company_id=@company_id
	AND
	company_type_id=@company_type_id

