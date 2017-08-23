
CREATE PROCEDURE [dbo].[usp_PCS_GetPrintPostByMediaMonth]
	@media_month_id INT
AS
BEGIN

	CREATE TABLE #posts (tam_post_id INT)
	INSERT INTO #posts
		SELECT 
				tpp.tam_post_id 
		  FROM 
				tam_post_proposals tpp (NOLOCK)
				JOIN proposals p (NOLOCK) ON p.id=tpp.posting_plan_proposal_id
					  AND p.posting_media_month_id=@media_month_id
		  GROUP BY
				tpp.tam_post_id
	            
	CREATE TABLE #num_alternate_demos (tam_post_id INT, num_alternate_demos SMALLINT)
	INSERT INTO #num_alternate_demos
		SELECT
				p.tam_post_id,
				COUNT (DISTINCT pa.audience_id)
		FROM
				#posts p
				JOIN tam_post_proposals tpp (NOLOCK) ON tpp.tam_post_id=p.tam_post_id
					  AND tpp.post_source_code=0
				JOIN proposal_audiences pa (NOLOCK) 
					ON pa.proposal_id=tpp.posting_plan_proposal_id
				JOIN proposals pr (NOLOCK)ON pr.id=tpp.posting_plan_proposal_id
		  WHERE
				pa.ordinal>1
		  GROUP BY
				p.tam_post_id
	    
	SELECT DISTINCT
		tp.id 'post id',
		tpgd.guaranteed_audience_code 'guaranteed audience code',
		nad.num_alternate_demos '# alternate demos',
		tp.produce_monthy_posts 'MP',
		tp.produce_quarterly_posts 'QP',
		tp.produce_full_posts 'FP',
		tp.post_setup_advertiser 'advertiser',
		tp.post_setup_product 'product',
		tp.title 'title',
		dp.sales_model_id 'sales model id'
	FROM
		  #posts p
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.tam_post_id=p.tam_post_id
				AND tpp.post_source_code = 0
		JOIN tam_posts tp (NOLOCK) ON tp.id=p.tam_post_id
				AND tp.is_deleted=0
		JOIN uvw_display_proposals dp ON dp.id=tpp.posting_plan_proposal_id
		JOIN uvw_tam_post_guaranteed_demos tpgd ON tpgd.tam_post_id=tp.id
		JOIN #num_alternate_demos nad ON nad.tam_post_id=tpp.tam_post_id
	ORDER BY
		tp.post_setup_advertiser,
		tp.post_setup_product

	DROP TABLE #posts      
	DROP TABLE #num_alternate_demos   

END