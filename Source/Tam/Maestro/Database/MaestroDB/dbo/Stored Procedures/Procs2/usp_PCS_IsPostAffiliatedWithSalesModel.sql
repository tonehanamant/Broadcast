-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/25/2011
-- Description:	
-- =============================================
-- usp_PCS_IsPostAffiliatedWithSalesModel 100001,1
CREATE PROCEDURE [dbo].[usp_PCS_IsPostAffiliatedWithSalesModel]
	@tam_post_id INT,
	@sales_model_id INT
AS
BEGIN
	DECLARE @num_records INT
	
	SELECT
		@num_records = COUNT(*)
	FROM
		tam_post_proposals tpp (NOLOCK)
		JOIN proposals p (NOLOCK) ON p.id = tpp.posting_plan_proposal_id
		JOIN media_months mm (NOLOCK) ON mm.id=p.posting_media_month_id
		JOIN proposal_sales_models psm (NOLOCK) ON psm.proposal_id=p.id
			AND psm.sales_model_id IN (SELECT id FROM dbo.udf_GetSalesModelsFromSalesModel(@sales_model_id))
	WHERE
		tpp.tam_post_id=@tam_post_id

	IF @num_records > 0
		SELECT CAST(1 AS BIT)
	ELSE
		SELECT CAST(0 AS BIT)
END
