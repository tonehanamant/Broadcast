-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_AutoValidation_GetMaterials]
	@media_month_id INT
AS
BEGIN
	SELECT 
		DISTINCT a.affidavit_copy 
	FROM 
		affidavits a (NOLOCK) 
	WHERE 
		a.media_month_id=@media_month_id
		AND a.material_id IS NULL
		 
END
