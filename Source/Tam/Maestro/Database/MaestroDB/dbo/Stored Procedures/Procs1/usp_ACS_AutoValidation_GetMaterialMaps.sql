-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
-- usp_ACS_GetMaterialMapsForAutoValidation 'Affidavits'
CREATE PROCEDURE [dbo].[usp_ACS_AutoValidation_GetMaterialMaps]
	@media_month_id INT,
	@map_set VARCHAR(15)
AS
BEGIN
	SELECT
		mm.*
	FROM
		material_maps mm (NOLOCK)
	WHERE
		mm.map_set=@map_set
		AND mm.map_value IN (
			SELECT 
				DISTINCT a.affidavit_copy 
			FROM 
				affidavits a (NOLOCK)
			WHERE 
				a.media_month_id=@media_month_id
				AND a.material_id IS NULL
		)
END
