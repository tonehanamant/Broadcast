CREATE PROCEDURE usp_cmw_traffic_companies_select_all
AS
SELECT
	*
FROM
	cmw_traffic_companies WITH(NOLOCK)
