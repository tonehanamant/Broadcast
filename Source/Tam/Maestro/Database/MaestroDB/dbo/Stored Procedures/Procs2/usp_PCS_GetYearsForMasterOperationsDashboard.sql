-- =============================================
-- Author:		Stephen DeFusco & Nicholas Kheynis
-- Create date: <8/27/2014>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetYearsForMasterOperationsDashboard]
AS
BEGIN
	SELECT DISTINCT      
		mm.year
	FROM
		dbo.tam_post_proposals tpp (NOLOCK)
		JOIN proposals p (NOLOCK) ON p.id=tpp.posting_plan_proposal_id
		JOIN dbo.media_months mm (NOLOCK) ON mm.id=p.posting_media_month_id
	ORDER BY
		mm.year DESC
END
