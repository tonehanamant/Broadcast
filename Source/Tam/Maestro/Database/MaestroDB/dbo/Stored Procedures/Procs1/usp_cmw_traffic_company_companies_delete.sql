CREATE PROCEDURE usp_cmw_traffic_company_companies_delete
(
	@child_cmw_traffic_company_id		Int,
	@parent_cmw_traffic_company_id		Int)
AS
DELETE FROM
	cmw_traffic_company_companies
WHERE
	child_cmw_traffic_company_id = @child_cmw_traffic_company_id
 AND
	parent_cmw_traffic_company_id = @parent_cmw_traffic_company_id
