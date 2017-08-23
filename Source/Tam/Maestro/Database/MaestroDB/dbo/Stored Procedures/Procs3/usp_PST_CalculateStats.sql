-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/5/2011
-- Description:	Calculates any stats for tam_posts and tam_post_proposals
-- =============================================
-- usp_PST_CalculateStats 1000000,18
CREATE PROCEDURE [dbo].[usp_PST_CalculateStats]
	@tam_post_id INT,
	@tam_post_proposal_id INT
AS
BEGIN	
	-- update tam_post_proposals
	UPDATE
		dbo.tam_post_proposals
	SET
		number_of_zones_delivering = (
			SELECT 
				COUNT(tpz.zone_id) 
			FROM 
				dbo.tam_post_zones tpz (NOLOCK) 
			WHERE 
				tpz.tam_post_proposal_id=@tam_post_proposal_id
		)
	WHERE
		id=@tam_post_proposal_id
		
	-- update tam_posts
	UPDATE
		dbo.tam_posts
	SET
		number_of_zones_delivering = (
			SELECT 
				COUNT(tpz.zone_id) 
			FROM 
				dbo.tam_post_zones tpz (NOLOCK)
				JOIN dbo.tam_post_proposals tpp (NOLOCK) ON tpp.id=tpz.tam_post_proposal_id
					AND tpp.tam_post_id=@tam_post_id
			WHERE 
				tpz.tam_post_proposal_id=@tam_post_proposal_id
		)
	WHERE
		id=@tam_post_proposal_id
END
