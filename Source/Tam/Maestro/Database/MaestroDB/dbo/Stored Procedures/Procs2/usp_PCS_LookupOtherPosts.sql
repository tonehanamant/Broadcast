-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/9/2010
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_LookupOtherPosts]
	@excluded_tam_post_id INT,
	@proposal_id INT
AS
BEGIN
	SELECT
		tpp.tam_post_id,
		tp.title,
		tpp.posting_plan_proposal_id
	FROM
		tam_post_proposals tpp (NOLOCK)
		JOIN tam_posts tp ON tp.id=tpp.tam_post_id
			AND tp.is_deleted=0
	WHERE
		tpp.tam_post_id <> @excluded_tam_post_id
		AND tpp.post_source_code = 0
		AND tpp.posting_plan_proposal_id = @proposal_id
END
