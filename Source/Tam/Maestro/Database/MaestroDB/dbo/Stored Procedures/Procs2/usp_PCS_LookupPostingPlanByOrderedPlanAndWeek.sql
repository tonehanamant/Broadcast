-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/24/2012
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_PCS_LookupPostingPlanByOrderedPlanAndWeek
	@ordered_proposal_id INT,
	@media_week_id INT
AS
BEGIN
	DECLARE @media_month_id INT
	SELECT @media_month_id = mw.media_month_id FROM media_weeks mw (NOLOCK) WHERE id=@media_week_id
	
	SELECT
		p.*
	FROM
		proposals p (NOLOCK)
	WHERE
		p.original_proposal_id=@ordered_proposal_id
		AND p.posting_media_month_id=@media_month_id
		AND p.id IN (
			SELECT DISTINCT posting_plan_proposal_id FROM tam_post_proposals tpp (NOLOCK)
		)
END
