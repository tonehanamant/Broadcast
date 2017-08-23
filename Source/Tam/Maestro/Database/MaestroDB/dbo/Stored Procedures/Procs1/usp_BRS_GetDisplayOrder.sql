

CREATE PROCEDURE [dbo].[usp_BRS_GetDisplayOrder]
@orderID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		cmw_traffic.id,
		cmw_traffic.sort_id,
		cmw_traffic.display_id,
		cmw_traffic.status_id,
		advertisers.[name] 'advertiser',
		products.[name] 'product',
		agencies.[name] 'agency',
		cmw_traffic.release_name 'title',
		cmw_traffic.flight_text,
		cmw_traffic.date_created,
		cmw_traffic.date_last_modified,
		cmw_traffic.start_date,
		cmw_traffic.end_date,
		systems.code
	FROM 
		uvw_cmw_traffic as cmw_traffic (NOLOCK)
		LEFT JOIN cmw_traffic_companies advertisers (NOLOCK) ON advertisers.id=cmw_traffic.advertiser_cmw_traffic_company_id 
		LEFT JOIN cmw_traffic_companies agencies (NOLOCK) ON agencies.id=cmw_traffic.agency_cmw_traffic_company_id 
		LEFT JOIN cmw_traffic_products products (NOLOCK) ON products.id = cmw_traffic.cmw_traffic_product_id
		LEFT JOIN systems (NOLOCK) ON systems.id = cmw_traffic.system_id
	WHERE
		cmw_traffic.id = @orderID
	ORDER BY 
		cmw_traffic.sort_id DESC
END

