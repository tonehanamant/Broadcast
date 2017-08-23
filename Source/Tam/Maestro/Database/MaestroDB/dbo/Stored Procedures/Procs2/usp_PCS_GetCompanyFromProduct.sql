
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetCompanyFromProduct]
	@product_id INT
AS
BEGIN
	SELECT 
		c.*
	FROM 
		companies c (NOLOCK) 
	WHERE 
		c.id IN (
			SELECT company_id FROM products (NOLOCK) WHERE id=@product_id
		)
END

