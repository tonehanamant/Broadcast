CREATE PROCEDURE usp_cmw_traffic_agencies_select
(
	@cmw_traffic_company_id		Int
)
AS
SELECT
	*
FROM
	cmw_traffic_agencies WITH(NOLOCK)
WHERE
	cmw_traffic_company_id=@cmw_traffic_company_id

