CREATE PROCEDURE usp_company_companies_insert
(
	@company_id		Int,
	@parent_company_id		Int
)
AS
INSERT INTO company_companies
(
	company_id,
	parent_company_id
)
VALUES
(
	@company_id,
	@parent_company_id
)

