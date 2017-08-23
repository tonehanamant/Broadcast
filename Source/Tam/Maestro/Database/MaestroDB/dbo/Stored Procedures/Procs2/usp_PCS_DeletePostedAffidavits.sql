-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/15/2010
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_DeletePostedAffidavits]
	@tam_post_proposal_id INT
AS
BEGIN
	DECLARE @media_month_id INT
	SELECT @media_month_id = p.posting_media_month_id FROM tam_post_proposals tpp (NOLOCK) JOIN proposals p (NOLOCK) ON p.id=tpp.posting_plan_proposal_id WHERE tpp.id=@tam_post_proposal_id
	
	DELETE FROM 
		posted_affidavits 
	WHERE
		media_month_id = @media_month_id
		AND tam_post_proposal_id = @tam_post_proposal_id
END
