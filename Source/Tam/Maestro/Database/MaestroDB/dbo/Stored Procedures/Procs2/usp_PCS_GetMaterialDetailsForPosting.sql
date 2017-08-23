-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetMaterialDetailsForPosting 25379
CREATE PROCEDURE [dbo].[usp_PCS_GetMaterialDetailsForPosting]
	@proposal_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	CREATE TABLE #tmp_materials (material_id INT, revised_material_id INT, start_date DATETIME, end_date DATETIME)
	CREATE INDEX IX_tmp_materials on #tmp_materials (material_id)

	INSERT INTO #tmp_materials
		SELECT pm.material_id, pm.material_id, pm.start_date, pm.end_date FROM proposal_materials pm (NOLOCK) WHERE proposal_id=@proposal_id
	INSERT INTO #tmp_materials
		SELECT mr.original_material_id, mr.revised_material_id, pm.start_date, pm.end_date FROM proposal_materials pm (NOLOCK) JOIN material_revisions mr (NOLOCK) ON mr.revised_material_id=pm.material_id WHERE pm.proposal_id=@proposal_id

    SELECT
		m.id,
		m_rev.id,
		m.code,
		m_rev.code,
		sl.delivery_multiplier
	FROM
		#tmp_materials tm
		JOIN materials m (NOLOCK) ON m.id=tm.material_id
		JOIN materials m_rev (NOLOCK) ON m_rev.id=tm.revised_material_id
		JOIN spot_lengths sl (NOLOCK) ON sl.id=m_rev.spot_length_id

	DROP TABLE #tmp_materials;
END
