-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/29/2011
-- Description:	
-- =============================================
-- usp_MCS_GetDisplayMaterialsForPosting NULL,NULL,NULL,NULL,NULL
CREATE PROCEDURE [dbo].[usp_MCS_GetDisplayMaterialsForPosting]
	@advertiser_id INT,
	@product_id INT,
	@start_date DATETIME,
	@end_date DATETIME,
	@trafficked_only BIT
AS
BEGIN
	SELECT 
		m.*
	FROM 
		uvw_display_materials m (NOLOCK)
		JOIN spot_lengths sl (NOLOCK) ON sl.id=m.spot_length_id
	WHERE
		m.type='Original'
		AND (@advertiser_id IS NULL OR m.product_id IN (SELECT id FROM products (NOLOCK) WHERE company_id = @advertiser_id))
		AND (@product_id IS NULL	OR m.product_id = @product_id)
		AND (@trafficked_only = 0 OR m.material_id IN (
			-- this gives us the married copy components that were trafficked AND that match the optional search creteria
			SELECT 
				m1.material_id
			FROM 
				traffic_materials tm (NOLOCK) 
				LEFT JOIN material_revisions mr (NOLOCK) ON mr.original_material_id=tm.material_id
				JOIN uvw_display_materials m1 (NOLOCK) ON m1.material_id=mr.revised_material_id
			WHERE
				m1.type='Original'
				AND m1.active=1
				AND (@advertiser_id IS NULL OR m1.product_id IN (SELECT id FROM products (NOLOCK) WHERE company_id = @advertiser_id))
				AND (@product_id IS NULL	OR m1.product_id = @product_id)
				AND (tm.start_date <= @end_date AND tm.end_date >= @start_date)
			
			UNION
			
			-- this gives us the unmarried copies that were trafficked AND that match the optional search creteria
			SELECT 
				m1.material_id
			FROM 
				traffic_materials tm (NOLOCK) 
				JOIN uvw_display_materials m1 (NOLOCK) ON m1.material_id=tm.material_id
			WHERE
				m1.type='Original'
				AND m1.active=1
				AND (@advertiser_id IS NULL OR m1.product_id IN (SELECT id FROM products (NOLOCK) WHERE company_id = @advertiser_id))
				AND (@product_id IS NULL	OR m1.product_id = @product_id)
				AND (tm.start_date <= @end_date AND tm.end_date >= @start_date)
			)
		)
	ORDER BY
		m.code
END
