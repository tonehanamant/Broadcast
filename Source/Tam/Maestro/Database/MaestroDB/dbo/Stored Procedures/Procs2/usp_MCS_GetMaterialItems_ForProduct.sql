-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/19/2011
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MCS_GetMaterialItems_ForProduct]
	@product_id INT
AS
BEGIN
	SELECT * FROM
	(
		SELECT 
			materials.id,
			materials.code,
			spot_lengths.length 
		FROM 
			materials (NOLOCK)
			JOIN spot_lengths (NOLOCK) ON spot_lengths.id=materials.spot_length_id
		WHERE
			product_id=@product_id

		UNION

		SELECT 
			materials.id,
			materials.code,
			spot_lengths.length 
		FROM 
			materials (NOLOCK)
			JOIN spot_lengths (NOLOCK) ON spot_lengths.id=materials.spot_length_id
		WHERE
			materials.id IN (
				SELECT 
					DISTINCT original_material_id 
				FROM 
					material_revisions mv	(NOLOCK)
					JOIN materials m		(NOLOCK) ON m.id=mv.revised_material_id
				WHERE
					m.product_id=@product_id 
			)
	) tmp
	ORDER BY 
		code
END
