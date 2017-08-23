
-- usp_PCS_GetDisplayAdvertiserProposals
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayAdvertiserProposals]
	@type TINYINT -- 0=cable, 1=hispanic old/new, 2=posting plans, 3=National DR, 4=IMW
AS
BEGIN
	SELECT DISTINCT
		p.advertiser_company_id as id,
		COUNT(*)
	FROM
		proposals p (NOLOCK)
	WHERE
		(@type=0	AND p.id IN (SELECT proposal_id FROM proposal_sales_models WHERE sales_model_id=1))
		OR (@type=1 AND p.id IN (SELECT proposal_id FROM proposal_sales_models WHERE sales_model_id IN (2,3)))
		OR (@type=2 AND p.proposal_status_id=7)
		OR (@type=3 AND p.id IN (SELECT proposal_id FROM proposal_sales_models WHERE sales_model_id IN (4)))
		OR (@type=4 AND p.id IN (SELECT proposal_id FROM proposal_sales_models WHERE sales_model_id IN (5)))
	GROUP BY
		p.advertiser_company_id
END
