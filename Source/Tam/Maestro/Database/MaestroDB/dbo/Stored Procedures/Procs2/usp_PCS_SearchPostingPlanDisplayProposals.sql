
-- Task 8626 :Statistical Tables for Married Plans to Improve Performance END



	-- =============================================
	-- Author:		Joshua Jewell
	-- Create date: 6/7/2010
	-- Description:	Searches posting plans.
	-- =============================================
	CREATE PROCEDURE [dbo].[usp_PCS_SearchPostingPlanDisplayProposals]
		@year INT,
		@quarter INT,
		@advertiser_company_id INT,
		@product_id INT,
		@campaign_id INT
	AS
	BEGIN
		-- SET NOCOUNT ON added to prevent extra result sets from
		-- interfering with SELECT statements.
		SET NOCOUNT ON;
	
	    SELECT
			dp.*
		FROM 
			uvw_display_proposals dp
			JOIN proposals p (NOLOCK) ON p.id=dp.id
			JOIN proposals po (NOLOCK) ON po.id=p.original_proposal_id
				AND po.proposal_status_id=4 -- ordered
			JOIN media_months mm (NOLOCK) ON mm.id=p.posting_media_month_id
		WHERE
			mm.[year] = @year 
			AND dp.proposal_status_id=7 -- posting plan
			AND CASE mm.[month] WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END = @quarter
			AND (@advertiser_company_id IS NULL OR p.advertiser_company_id = @advertiser_company_id)
			AND (@product_id IS NULL OR p.product_id = @product_id)
			AND p.campaign_id = @campaign_id
		ORDER BY 
			dp.id DESC
	END
