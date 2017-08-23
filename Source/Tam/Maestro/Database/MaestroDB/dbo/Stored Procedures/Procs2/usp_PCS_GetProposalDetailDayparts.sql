-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/27/2011
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetProposalDetailDayparts]
	@tam_post_id INT
AS
BEGIN
	SELECT DISTINCT
		pd.id,
		pd.daypart_id 
	FROM
		tam_post_proposals tpp (NOLOCK)
		JOIN proposal_details pd (NOLOCK) ON pd.proposal_id=tpp.posting_plan_proposal_id
	WHERE
		tpp.tam_post_id=@tam_post_id
		
	SELECT DISTINCT
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
		tam_post_proposals tpp (NOLOCK)
		JOIN proposal_details pd (NOLOCK) ON pd.proposal_id=tpp.posting_plan_proposal_id
		JOIN vw_ccc_daypart d ON d.id=pd.daypart_id
	WHERE
		tpp.tam_post_id=@tam_post_id
END
