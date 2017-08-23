

CREATE PROCEDURE [dbo].[usp_BRS_GetAdvertiserByMaterialID]
@materialID int
AS
BEGIN

	SET NOCOUNT ON;
	SELECT 
		cmw_traffic_companies.id,
		cmw_traffic_companies.[name]
	FROM
		cmw_materials (nolock)
	JOIN
		cmw_traffic_products (nolock) on cmw_materials.cmw_traffic_product_id = cmw_traffic_products.id
	JOIN
		cmw_traffic_companies (nolock) on cmw_traffic_products.cmw_traffic_advertisers_id = cmw_traffic_companies.id
	WHERE
		cmw_materials.id = @materialID
END
