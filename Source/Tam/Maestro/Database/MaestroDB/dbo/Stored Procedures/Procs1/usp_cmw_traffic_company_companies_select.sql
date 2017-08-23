CREATE PROCEDURE usp_cmw_traffic_company_companies_select
(
	@child_cmw_traffic_company_id		Int,
	@parent_cmw_traffic_company_id		Int
)
AS
SELECT
	*
FROM
	cmw_traffic_company_companies WITH(NOLOCK)
WHERE
	child_cmw_traffic_company_id=@child_cmw_traffic_company_id
	AND
	parent_cmw_traffic_company_id=@parent_cmw_traffic_company_id

