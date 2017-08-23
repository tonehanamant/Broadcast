-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/20/2010
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetDisplayPostsByAdvertiserAndSalesModel 40121,1
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayPostsByAdvertiserAndSalesModel]
	@advertiser_company_id INT,
	@sales_model_id INT
AS
BEGIN
	SELECT
		dp.*
	FROM
		uvw_display_posts dp
	WHERE
		dp.advertiser_company_id=@advertiser_company_id
		AND dp.sales_model_id IN (SELECT id FROM dbo.udf_GetSalesModelsFromSalesModel(@sales_model_id))
END
