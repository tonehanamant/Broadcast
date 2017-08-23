CREATE PROCEDURE usp_cmw_traffic_advertisers_delete
(
	@cmw_traffic_company_id		Int)
AS
DELETE FROM
	cmw_traffic_advertisers
WHERE
	cmw_traffic_company_id = @cmw_traffic_company_id
