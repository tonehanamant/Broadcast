
CREATE PROCEDURE usp_PCS_GetPostedAdvertiserItemsByYear
	@year INT
AS
BEGIN
	SELECT DISTINCT
		p.advertiser_company_id
	FROM
		tam_posts tp (NOLOCK)
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.tam_post_id=tp.id
		JOIN proposals p (NOLOCK) ON p.id=tpp.posting_plan_proposal_id
		JOIN media_months mm (NOLOCK) ON mm.id=p.posting_media_month_id
			AND mm.[year] = @year
END
