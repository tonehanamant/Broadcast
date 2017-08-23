-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/24/2012
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_BRS_GetCmwTrafficProductDescriptions
AS
BEGIN
	SELECT 
		ctpd.* 
	FROM 
		cmw_traffic_product_descriptions ctpd (NOLOCK) 
	ORDER BY 
		ctpd.product_description
END
