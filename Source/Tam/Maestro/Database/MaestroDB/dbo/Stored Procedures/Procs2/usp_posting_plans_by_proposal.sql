CREATE PROCEDURE usp_posting_plans_by_proposal
(
	@tam_post_id int
)
AS
BEGIN
	SELECT DISTINCT p.original_proposal_id
	FROM tam_post_proposals tpp
	INNER JOIN tam_posts tp ON tpp.tam_post_id = tp.id
	INNER JOIN proposals p ON tpp.posting_plan_proposal_id = p.id
	WHERE tam_post_id = @tam_post_id

END

