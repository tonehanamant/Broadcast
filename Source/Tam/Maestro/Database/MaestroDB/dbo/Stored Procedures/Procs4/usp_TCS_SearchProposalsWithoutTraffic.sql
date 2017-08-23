-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/5/2014
-- Description:	
-- =============================================
-- usp_TCS_SearchProposalsWithoutTraffic 'Illinois',NULL,NULL,NULL,NULL,NULL,NULL
-- usp_TCS_SearchProposalsWithoutTraffic '',NULL,NULL,NULL,NULL,'OR',NULL
-- usp_TCS_SearchProposalsWithoutTraffic '',NULL,NULL,NULL,NULL,NULL,NULL
CREATE PROCEDURE [dbo].[usp_TCS_SearchProposalsWithoutTraffic]
	@search_text VARCHAR(255),
	@start_date DATE,
	@end_date DATE,
	@spot_length_ids VARCHAR(MAX),
	@rate_card_type_ids VARCHAR(MAX),
	@proposal_status_codes VARCHAR(MAX),
	@audience_id INT
AS
BEGIN
	DECLARE @num_search_results INT
	SET @num_search_results = 2000
	
	IF @search_text IS NOT NULL AND LEN(@search_text)>0
		SET @search_text = '%' + @search_text + '%';
	ELSE
		SET @search_text = NULL
	
	
	IF @search_text IS NULL AND @start_date IS NULL AND @end_date IS NULL AND @spot_length_ids IS NULL AND @rate_card_type_ids IS NULL AND @proposal_status_codes IS NULL AND @audience_id IS NULL 
		SET @num_search_results = 500
		
	SELECT TOP (@num_search_results)
		dp.id,
		dp.version_number,
		dp.total_gross_cost,
		dp.advertiser,
		dp.product,
		dp.agency,
		dp.title,
		dp.salesperson,
		dp.flight_text,
		dp.include_on_availability_planner,
		dp.date_created,
		dp.date_last_modified,
		ps.name,
		dp.length,
		rct.name,
		ISNULL(dp.original_proposal_id, dp.id) 'original_proposal_id'
	FROM
		dbo.uvw_display_proposals dp
		JOIN proposals p (NOLOCK) ON p.id=dp.id
		JOIN dbo.proposal_statuses ps (NOLOCK) ON ps.id=p.proposal_status_id
		JOIN dbo.rate_card_types rct (NOLOCK) ON rct.id=dp.rate_card_type_id
		LEFT JOIN dbo.proposal_audiences pa_hh (NOLOCK) ON pa_hh.proposal_id=p.id AND pa_hh.ordinal=0
		LEFT JOIN dbo.proposal_audiences pa_pr (NOLOCK) ON pa_pr.proposal_id=p.id AND pa_pr.ordinal=1
	WHERE
		dp.sales_model_id <> 4
		AND dp.proposal_status_id NOT IN (5,6,7) -- previously ordered, cancelled before start, posting plan
		AND (@search_text IS NULL OR (dp.title LIKE @search_text OR dp.agency LIKE @search_text OR dp.advertiser LIKE @search_text OR dp.product LIKE @search_text))
		AND (@start_date IS NULL OR dp.start_date >= @start_date)
		AND (@end_date IS NULL OR dp.end_date <= @end_date)
		AND (@spot_length_ids IS NULL OR p.default_spot_length_id IN (SELECT id FROM dbo.SplitIntegers(@spot_length_ids)))
		AND (@rate_card_type_ids IS NULL OR p.rate_card_type_id IN (SELECT id FROM dbo.SplitIntegers(@rate_card_type_ids)))
		AND (@proposal_status_codes IS NULL OR ps.code IN (SELECT StringPart FROM dbo.SplitString(@proposal_status_codes,',')))
		AND (@audience_id IS NULL OR (p.guarantee_type=0 AND pa_hh.audience_id=@audience_id) OR (p.guarantee_type=1 AND pa_pr.audience_id=@audience_id))
		AND dp.id NOT IN (
			SELECT proposal_id FROM traffic_proposals tp (NOLOCK)
		)
	ORDER BY
		dp.id DESC
END
