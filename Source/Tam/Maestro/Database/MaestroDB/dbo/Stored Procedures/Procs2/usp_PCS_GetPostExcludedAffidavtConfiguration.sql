-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/2/2011
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetPostExcludedAffidavtConfiguration]
	@tam_post_id INT
AS
BEGIN
	DECLARE @effective_date DATETIME
	SELECT 
		@effective_date = MIN(p.start_date)
	FROM
		tam_posts tp (NOLOCK)
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.tam_post_id=tp.id
		JOIN proposals p (NOLOCK) ON p.id=tpp.posting_plan_proposal_id
	WHERE
		tp.id=@tam_post_id
		
	SELECT
		m.code 'material',
		n.code 'network',
		CASE WHEN p.media_month IS NULL THEN '[ALL]' ELSE p.media_month + ' - ' + p.title END 'proposal',
		p.id,
		tpea.*
	FROM
		tam_post_excluded_affidavits tpea (NOLOCK)
		JOIN tam_posts tp (NOLOCK) ON tp.id=tpea.tam_post_id
		LEFT JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpea.tam_post_proposal_id
		LEFT JOIN uvw_display_proposals p (NOLOCK) ON p.id=tpp.posting_plan_proposal_id
		LEFT JOIN materials m (NOLOCK) ON m.id=tpea.material_id
		LEFT JOIN uvw_network_universe n ON n.network_id=tpea.network_id AND (n.start_date<=@effective_date AND (n.end_date>=@effective_date OR n.end_date IS NULL)) 
	WHERE
		tpea.tam_post_id=@tam_post_id
		
	SELECT
		s.code 'system',
		tpeas.*
	FROM
		tam_post_excluded_affidavit_systems tpeas (NOLOCK)
		JOIN tam_post_excluded_affidavits tpea (NOLOCK) ON tpea.id=tpeas.tam_post_excluded_affidavit_id
			AND tpea.tam_post_id=@tam_post_id
		JOIN uvw_system_universe s ON s.system_id=tpeas.system_id AND (s.start_date<=@effective_date AND (s.end_date>=@effective_date OR s.end_date IS NULL)) 
	ORDER BY
		s.code
END
