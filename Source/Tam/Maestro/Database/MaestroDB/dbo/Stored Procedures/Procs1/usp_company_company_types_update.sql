CREATE PROCEDURE usp_company_company_types_update
(
	@company_id		Int,
	@company_type_id		Int,
	@effective_date		DateTime
)
AS
UPDATE company_company_types SET
	effective_date = @effective_date
WHERE
	company_id = @company_id AND
	company_type_id = @company_type_id
