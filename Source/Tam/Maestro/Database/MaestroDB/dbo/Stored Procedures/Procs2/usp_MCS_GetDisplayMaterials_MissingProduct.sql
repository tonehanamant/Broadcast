-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/19/2011
-- Description:	<Description,,>
-- =============================================
-- usp_MCS_GetDisplayMaterials_MissingProduct
CREATE PROCEDURE [dbo].[usp_MCS_GetDisplayMaterials_MissingProduct]
AS
BEGIN
    SELECT
		dm.*
	FROM
		uvw_display_materials dm
	WHERE
		dm.type<>'Married' 
		AND dm.product_id IS NULL
	ORDER BY
		dm.code
END
