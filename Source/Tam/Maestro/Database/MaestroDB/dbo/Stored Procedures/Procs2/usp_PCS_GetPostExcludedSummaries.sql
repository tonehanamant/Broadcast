-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/11/2011
-- Description:	Returns the 
-- =============================================
CREATE PROCEDURE usp_PCS_GetPostExcludedSummaries
	@tam_post_id INT
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
END
