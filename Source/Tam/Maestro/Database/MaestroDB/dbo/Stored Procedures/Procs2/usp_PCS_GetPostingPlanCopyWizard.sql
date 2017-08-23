-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/10/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_PCS_GetPostingPlanCopyWizard
	@media_month_id INT
AS
BEGIN
	SET NOCOUNT ON;

	-- posting plans in month
    SELECT dp.* FROM uvw_display_proposals dp WHERE dp.posting_media_month_id=@media_month_id
	
	-- materials in posting plans
	SELECT
		pm.proposal_id,
		dm.*
	FROM
		proposals p (NOLOCK)
		JOIN proposal_materials pm (NOLOCK) ON pm.proposal_id=p.id
		JOIN uvw_display_materials dm ON dm.material_id=pm.material_id
	WHERE
		p.posting_media_month_id=@media_month_id
END
