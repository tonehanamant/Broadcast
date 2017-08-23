

CREATE PROCEDURE [dbo].[usp_BRS_GetDisplayMaterialsForAdvertiser]
@advertiserID int
AS
BEGIN

	SET NOCOUNT ON;
	SELECT
		cmw_materials.id,
		spot_lengths.length,
		cmw_traffic_products.[name] 'Product',
		cmw_traffic_companies.[name] 'Advertiser',
		code,
		title,
		cmw_materials.date_created,
		cmw_materials.date_last_modified		
	FROM
		cmw_materials (nolock)
	JOIN
		spot_lengths (nolock) on cmw_materials.spot_length_id = spot_lengths.id
	LEFT JOIN
		cmw_traffic_products (nolock) on cmw_materials.cmw_traffic_product_id = cmw_traffic_products.id
	JOIN
		cmw_traffic_companies (nolock) on cmw_traffic_companies.id = cmw_traffic_products.cmw_traffic_advertisers_id
	WHERE
		cmw_traffic_companies.id = @advertiserID
END




