


CREATE PROCEDURE [dbo].[usp_BRS_GetOrdersByProductIDs]
@IDs varchar(MAX)

AS
BEGIN

	SET NOCOUNT ON;
	SELECT
	id,
	agency_cmw_traffic_company_id,
	advertiser_cmw_traffic_company_id,
	cmw_traffic_product_id,
	system_id,
	zone_id,
	network_id,
	status_id,
	cmw_traffic_product_description_id,
	coverage_universe,
	order_date,
	release_name,
	release_date,
	start_date,
	end_date,
	notes,
	flight_text,
	network_handles_copy,
	date_created,
	date_last_modified,
	salesperson_employee_id,
	original_cmw_traffic_id,
	approved_by_employee_id,
	approved_date
FROM
	cmw_traffic (NOLOCK)
WHERE
	cmw_traffic_product_id in (select id from dbo.SplitIntegers(@IDs))
END


