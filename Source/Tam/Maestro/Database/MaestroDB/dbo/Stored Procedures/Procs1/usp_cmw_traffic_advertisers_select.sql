CREATE PROCEDURE usp_cmw_traffic_advertisers_select
(
	@cmw_traffic_company_id		Int
)
AS
SELECT
	*
FROM
	cmw_traffic_advertisers WITH(NOLOCK)
WHERE
	cmw_traffic_company_id=@cmw_traffic_company_id

