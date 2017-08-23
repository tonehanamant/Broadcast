-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	Returns tam_post_proposals.id's based on the year/quarter/month paramters which:
--				- is MSA (post_source_code=2) and has been posted and aggregated OR is the TAM record (post_source_code=0)
--				The point is for a tam_post_id and posting_plan_proposal_id to return the BEST data (tam_post_proposals.id) available.
--				NOTE: This ONLY filters out all tam_posts marked "is_deleted". It does include "Spec" and "Official" posts as well as posts marked "Exclude from Year to Date".
-- =============================================
-- SELECT * FROM dbo.udf_GetTamPostDemoInfo(1003206)
CREATE FUNCTION [dbo].[udf_GetTamPostDemoInfo]
(	
	@tam_post_id INT
)
RETURNS TABLE 
AS
RETURN
(
	SELECT
		MIN(pag.audience_id) 'guaranteed_audience_id',
		MIN(pap.audience_id) 'primary_audience_id'
	FROM
		tam_post_proposals tpp			 (NOLOCK)
		JOIN proposals p				 (NOLOCK) ON p.id=tpp.posting_plan_proposal_id
		LEFT JOIN proposal_audiences pag (NOLOCK) ON pag.proposal_id=tpp.posting_plan_proposal_id
			AND pag.ordinal=p.guarantee_type -- when guarantee_type = 0 then Households when guarantee_type = 0 then Primary Demo, just so happens the proposal_audiences.ordinal field corresponds with guarantee_type, proposal_audiences.ordinal=0=HouseHolds, when proposal_audiences.ordinal=1=Primary Demo
		LEFT JOIN proposal_audiences pap (NOLOCK) ON pap.proposal_id=tpp.posting_plan_proposal_id
			AND pap.ordinal=1
	WHERE
		tpp.tam_post_id=@tam_post_id
)
