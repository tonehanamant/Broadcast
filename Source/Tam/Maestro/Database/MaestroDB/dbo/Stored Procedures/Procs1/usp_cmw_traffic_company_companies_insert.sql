CREATE PROCEDURE usp_cmw_traffic_company_companies_insert
(
	@child_cmw_traffic_company_id		Int,
	@parent_cmw_traffic_company_id		Int
)
AS
INSERT INTO cmw_traffic_company_companies
(
	child_cmw_traffic_company_id,
	parent_cmw_traffic_company_id
)
VALUES
(
	@child_cmw_traffic_company_id,
	@parent_cmw_traffic_company_id
)

