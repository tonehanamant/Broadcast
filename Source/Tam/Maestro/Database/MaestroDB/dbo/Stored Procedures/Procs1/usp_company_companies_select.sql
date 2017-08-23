CREATE PROCEDURE usp_company_companies_select
(
	@company_id		Int,
	@parent_company_id		Int
)
AS
SELECT
	*
FROM
	company_companies WITH(NOLOCK)
WHERE
	company_id=@company_id
	AND
	parent_company_id=@parent_company_id

