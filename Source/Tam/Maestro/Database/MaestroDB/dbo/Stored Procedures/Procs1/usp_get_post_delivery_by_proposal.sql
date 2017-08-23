CREATE PROCEDURE usp_get_post_delivery_by_proposal
	@proposal_id INT      
	,@tam_post_id INT   
AS
BEGIN
	   
	SELECT (SUM(CASE tp.rate_card_type_id WHEN 1 THEN tpnda.delivery ELSE tpnda.dr_delivery END) / 1000.0) 'delivery'       
	FROM tam_posts tp (NOLOCK)
	INNER JOIN tam_post_proposals tpp (NOLOCK) ON tpp.tam_post_id=tp.id    
		AND tpp.post_source_code = CASE WHEN tp.is_msa = 1 THEN 2 ELSE 0 END -- 0 = TAM POST, 1 = FAST TRACK and 2 = MSA
	INNER JOIN proposals p (NOLOCK) ON p.id=tpp.posting_plan_proposal_id    
		AND p.original_proposal_id=@proposal_id    
	INNER JOIN tam_post_network_details tpnd (NOLOCK) ON tpnd.tam_post_proposal_id=tpp.id  
	INNER JOIN tam_post_network_detail_audiences tpnda (NOLOCK) ON tpnda.tam_post_network_detail_id=tpnd.id
	WHERE tp.is_deleted=0    
		AND tp.post_type_code=1    -- 0=spec, 1=official
		AND tp.exclude_from_year_to_date_report=0    
		AND tp.id = @tam_post_id
END
