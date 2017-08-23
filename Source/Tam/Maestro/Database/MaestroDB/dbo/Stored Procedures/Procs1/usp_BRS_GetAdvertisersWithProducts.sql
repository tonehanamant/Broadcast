



CREATE PROCEDURE [dbo].[usp_BRS_GetAdvertisersWithProducts]

AS
BEGIN

	SET NOCOUNT ON;
	SELECT DISTINCT 
		cmw_traffic_companies.id,
		cmw_traffic_companies.[name]
	FROM
		cmw_traffic_companies (nolock)
	JOIN
		cmw_traffic_advertisers (nolock) on cmw_traffic_companies.id = cmw_traffic_advertisers.cmw_traffic_company_id
	JOIN
		cmw_traffic_products (nolock) on cmw_traffic_advertisers.cmw_traffic_company_id = cmw_traffic_products.cmw_traffic_advertisers_id
	ORDER BY cmw_traffic_companies.[name] ASC
END
