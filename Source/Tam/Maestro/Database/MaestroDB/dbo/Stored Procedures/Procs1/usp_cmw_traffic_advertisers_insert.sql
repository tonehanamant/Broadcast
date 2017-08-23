CREATE PROCEDURE usp_cmw_traffic_advertisers_insert
(
	@cmw_traffic_company_id		Int
)
AS
INSERT INTO cmw_traffic_advertisers
(
	cmw_traffic_company_id
)
VALUES
(
	@cmw_traffic_company_id
)

