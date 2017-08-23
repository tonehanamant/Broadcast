-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/15/2015
-- Description:	Used to populate MSA Export file "contract_detail_demos.txt"
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_MsaExport_ContractDetailDemos]
	@msa_tam_post_proposal_ids UniqueIdTable READONLY
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    SELECT
		ROW_NUMBER() OVER (ORDER BY tpp.tam_post_id, pd.id, pa.ordinal) 'id',
		tpp.tam_post_id 'con_id',
		pd.id 'line_number',
		pa.ordinal 'demo_ord',
		CASE p.is_equivalized 
			WHEN 0 THEN  
				pda.rating * (pda.us_universe * pd.universal_scaling_factor)/1000.0 
			ELSE
				pda.rating * ((pda.us_universe * pd.universal_scaling_factor)/1000.0) * sl.delivery_multiplier 
		END 'imp',
		pda.rating * 100.0 'rating'
	FROM
		@msa_tam_post_proposal_ids mttp
		JOIN tam_post_proposals tpp ON tpp.id=mttp.id
		JOIN proposals p ON p.id=tpp.posting_plan_proposal_id
		JOIN proposal_details pd ON pd.proposal_id=tpp.posting_plan_proposal_id
		JOIN spot_lengths sl ON sl.id=pd.spot_length_id
		JOIN proposal_audiences pa ON pa.proposal_id=pd.proposal_id
		JOIN proposal_detail_audiences pda ON pd.id=pda.proposal_detail_id	
			AND pda.audience_id=pa.audience_id
	WHERE
		pa.ordinal BETWEEN 1 AND 5
	ORDER BY
		tpp.tam_post_id,
		pd.id,
		pa.ordinal
END