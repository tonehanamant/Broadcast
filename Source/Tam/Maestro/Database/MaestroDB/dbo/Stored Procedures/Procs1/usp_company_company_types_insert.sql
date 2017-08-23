CREATE PROCEDURE usp_company_company_types_insert
(
	@company_id		Int,
	@company_type_id		Int,
	@effective_date		DateTime
)
AS
INSERT INTO company_company_types
(
	company_id,
	company_type_id,
	effective_date
)
VALUES
(
	@company_id,
	@company_type_id,
	@effective_date
)

