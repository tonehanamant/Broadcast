CREATE PROCEDURE usp_company_companies_delete
(
	@company_id		Int,
	@parent_company_id		Int)
AS
DELETE FROM
	company_companies
WHERE
	company_id = @company_id
 AND
	parent_company_id = @parent_company_id
