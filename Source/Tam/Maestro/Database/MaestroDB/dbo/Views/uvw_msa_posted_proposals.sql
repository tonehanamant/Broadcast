CREATE VIEW [dbo].[uvw_msa_posted_proposals]
AS
	SELECT DISTINCT
		tpp.posting_plan_proposal_id 'proposal_id'		
	FROM 
		dbo.tam_post_proposals tpp WITH(NOLOCK) 
	WHERE 
		tpp.post_completed IS NOT NULL
		AND tpp.post_source_code = 2
