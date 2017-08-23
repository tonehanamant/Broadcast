CREATE VIEW uvw_media_ocean_posts
AS
	SELECT tpp.tam_post_id
	FROM tam_post_proposals tpp
	INNER JOIN proposals p ON p.id = tpp.posting_plan_proposal_id
	WHERE p.is_media_ocean_plan = 1
	GROUP BY tpp.tam_post_id
	
