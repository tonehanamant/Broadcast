-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/22/2010
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetProposalDetailDataToFindOverlap 338
CREATE PROCEDURE [dbo].[usp_PCS_GetProposalDetailDataToFindOverlap]
	@media_month_id INT
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

	SELECT 
		pd.proposal_id,
		pd.id,
		pd.network_id,
		pd.start_date,
		pd.end_date,
		pf.start_date,
		pf.end_date,
		pm.material_id,
		pm.start_date,
		pm.end_date,
		d.id,
		d.code,
		d.name,
		d.start_time,
		d.end_time,
		d.mon,
		d.tue,
		d.wed,
		d.thu,
		d.fri,
		d.sat,
		d.sun 
	FROM
		proposal_details pd
		JOIN proposals p ON p.id=pd.proposal_id
			AND p.proposal_status_id=7 
			AND p.posting_media_month_id=@media_month_id
		JOIN vw_ccc_daypart d ON d.id=pd.daypart_id 
		JOIN proposal_flights pf ON pf.proposal_id=pd.proposal_id
			AND pf.selected=1 
		JOIN proposal_materials pm ON pm.proposal_id=pd.proposal_id
		JOIN tam_post_proposals tpp ON tpp.posting_plan_proposal_id=pd.proposal_id 
			AND tpp.post_completed IS NOT NULL
		JOIN tam_posts tp ON tp.id=tpp.tam_post_id
			AND tp.post_type_code=1
END
