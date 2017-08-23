-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 2/3/2016
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetDistinctDaypartOfProposalsInPost]
	@tam_post_id INT
AS
BEGIN
	SELECT DISTINCT
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
		vw_ccc_daypart d
		JOIN proposal_details pd (NOLOCK) ON pd.daypart_id=d.id
			AND pd.proposal_id IN (
				SELECT DISTINCT posting_plan_proposal_id FROM tam_post_proposals tpp (NOLOCK) WHERE tpp.tam_post_id=@tam_post_id
			)
END