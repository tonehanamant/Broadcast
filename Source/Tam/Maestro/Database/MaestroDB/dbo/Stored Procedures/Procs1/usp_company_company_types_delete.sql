CREATE PROCEDURE usp_company_company_types_delete
(
	@company_id		Int,
	@company_type_id		Int)
AS
DELETE FROM
	company_company_types
WHERE
	company_id = @company_id
 AND
	company_type_id = @company_type_id
