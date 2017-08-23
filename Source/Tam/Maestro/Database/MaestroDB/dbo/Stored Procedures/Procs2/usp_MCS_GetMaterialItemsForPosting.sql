
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/14/2010
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MCS_GetMaterialItemsForPosting]
--	@ordered_proposal_id INT,
	@advertiser_id INT,
	@product_id INT,
	@start_date DATETIME,
	@end_date DATETIME,
	@trafficked_only BIT
AS
BEGIN
	SELECT 
		m.id,
		m.code,
		sl.length 
	FROM 
		materials m (NOLOCK)
		JOIN spot_lengths sl (NOLOCK) ON sl.id=m.spot_length_id
	WHERE
		m.type='Original'
		AND (@advertiser_id IS NULL OR m.product_id IN (SELECT id FROM products (NOLOCK) WHERE company_id = @advertiser_id))
		AND (@product_id IS NULL	OR m.product_id = @product_id)
		AND (@trafficked_only = 0 OR m.id IN (
			-- this gives us the married copy components that were trafficked AND that match the optional search creteria
			SELECT 
				m1.id
			FROM 
				traffic_materials tm WITH (NOLOCK) 
				JOIN reel_materials rm WITH (NOLOCK) on tm.reel_material_id = rm.id
				LEFT JOIN material_revisions mr WITH (NOLOCK) ON mr.original_material_id=rm.material_id
				JOIN materials m1 (NOLOCK) ON m1.id=mr.revised_material_id
			WHERE
				m1.type='Original'
				AND (@advertiser_id IS NULL OR m1.product_id IN (SELECT id FROM products (NOLOCK) WHERE company_id = @advertiser_id))
				AND (@product_id IS NULL	OR m1.product_id = @product_id)
				AND (tm.start_date <= @end_date AND tm.end_date >= @start_date)
			
			UNION
			
			-- this gives us the unmarried copies that were trafficked AND that match the optional search creteria
			SELECT 
				m1.id
			FROM 
				traffic_materials tm WITH (NOLOCK) 
				JOIN reel_materials rm WITH (NOLOCK) on tm.reel_material_id = rm.id
				JOIN materials m1 WITH (NOLOCK) ON m1.id=rm.material_id
			WHERE
				m1.type='Original'
				AND (@advertiser_id IS NULL OR m1.product_id IN (SELECT id FROM products (NOLOCK) WHERE company_id = @advertiser_id))
				AND (@product_id IS NULL	OR m1.product_id = @product_id)
				AND (tm.start_date <= @end_date AND tm.end_date >= @start_date)
			)
		)
	ORDER BY
		m.code

--	UNION
--
--	SELECT 
--		m.id,
--		m.code,
--		sl.length 
--	FROM 
--		materials m (NOLOCK)
--		JOIN spot_lengths sl (NOLOCK) ON sl.id=m.spot_length_id
--	WHERE
--		m.type='Original'
--		AND m.id IN (
--			SELECT material_id FROM traffic_materials tm (NOLOCK) WHERE tm.traffic_id IN (
--					SELECT traffic_id FROM traffic_proposals tp (NOLOCK) WHERE tp.proposal_id=@ordered_proposal_id
--				)
--			)
END

