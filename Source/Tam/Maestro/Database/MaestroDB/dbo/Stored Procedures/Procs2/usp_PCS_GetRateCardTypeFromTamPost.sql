-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/23/2010
-- Description:	Gets the rate card type of the ordered plan(s) given the tam_posts.id.
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetRateCardTypeFromTamPost]
	@tam_post_id INT
AS
BEGIN
	SELECT DISTINCT TOP 1 rate_card_type_id FROM proposals (NOLOCK) WHERE id IN (
		SELECT posting_plan_proposal_id FROM tam_post_proposals (NOLOCK) WHERE tam_post_id=@tam_post_id
	)
END
