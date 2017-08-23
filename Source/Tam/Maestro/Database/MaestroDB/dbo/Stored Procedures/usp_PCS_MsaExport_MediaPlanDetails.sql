-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/15/2015
-- Description:	Used to populate MSA Export file "media_plan_details.txt"
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_MsaExport_MediaPlanDetails]
	@msa_tam_post_proposal_ids UniqueIdTable READONLY
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    SELECT
		tpp.tam_post_id 'con_id',
		pd.id 'line_number',
		pd.network_id 'net_id',
		pd.num_spots 'units',
		pd.proposal_rate 'rate',
		pd.topography_universe 'universe',
		CASE p.is_equivalized 
			WHEN 0 THEN  
				pda.rating * (pda.us_universe * pd.universal_scaling_factor)/1000.0 
			ELSE
				pda.rating * ((pda.us_universe * pd.universal_scaling_factor)/1000.0) * sl.delivery_multiplier 
		END 'hh_imp',
		pda.rating * 100.0 'hh_rating'
	FROM
		@msa_tam_post_proposal_ids mttp
		JOIN tam_post_proposals tpp ON tpp.id=mttp.id
		JOIN proposals p ON p.id=tpp.posting_plan_proposal_id
		JOIN proposal_details pd ON pd.proposal_id=tpp.posting_plan_proposal_id
		JOIN proposal_detail_audiences pda ON pd.id=pda.proposal_detail_id	
			AND pda.audience_id=31
		JOIN spot_lengths sl ON sl.id=pd.spot_length_id
	ORDER BY
		tpp.tam_post_id,
		pd.id
END