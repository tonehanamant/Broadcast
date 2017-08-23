-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/6/2011
-- Description:	Returns the excluded portion of a post based on passed parameters.
-- =============================================
-- usp_PCS_GetPostExcludedSummaryResults 8,NULL,486,NULL,NULL
-- usp_PCS_GetPostExcludedSummaryResults 8,19,129,2,NULL
-- usp_PCS_GetPostExcludedSummaryResults 8,NULL,NULL,2,NULL
CREATE PROCEDURE [dbo].[usp_PCS_GetPostExcludedSummaryResults]
	@tam_post_id INT,
	@tam_post_proposal_id INT,
	@system_ids VARCHAR(MAX),
	@network_id INT,
	@material_id INT
AS
BEGIN
	SELECT
		tpes.id,
		tpes.tam_post_proposal_id,
		tpes.system_id,
		tpes.network_id,
		tpes.material_id,
		CASE tpesa.audience_id WHEN 31 THEN tpes.subscribers ELSE 0 END 'subscribers',
		CASE tpesa.audience_id WHEN 31 THEN tpes.units ELSE 0 END 'units',
		tpesa.audience_id,
		tpesa.delivery,
		tpesa.eq_delivery,
		tpesa.dr_delivery,
		tpesa.dr_eq_delivery
	FROM
		tam_post_exclusion_summaries tpes (NOLOCK)
		JOIN tam_post_exclusion_summary_audiences tpesa (NOLOCK) ON tpesa.tam_post_exclusion_summary_id=tpes.id
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpes.tam_post_proposal_id
			AND tpp.tam_post_id=@tam_post_id
	WHERE
		(@tam_post_proposal_id IS NULL	OR @tam_post_proposal_id=tpes.tam_post_proposal_id)
		AND (@system_ids IS NULL		OR tpes.system_id IN (SELECT id FROM dbo.SplitIntegers(@system_ids)))
		AND (@network_id IS NULL		OR @network_id=tpes.network_id)
		AND (@material_id IS NULL		OR @material_id=tpes.material_id)
END
