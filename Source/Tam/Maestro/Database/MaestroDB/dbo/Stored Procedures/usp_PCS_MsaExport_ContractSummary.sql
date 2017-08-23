-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/28/2015
-- Description:	Used to populate MSA Export "contract_summary.xlsx" support file.
-- =============================================
/*
	DECLARE @msa_tam_post_proposal_ids UniqueIdTable;
	INSERT INTO @msa_tam_post_proposal_ids SELECT 1003487
	INSERT INTO @msa_tam_post_proposal_ids SELECT 1003609
	EXEC usp_PCS_MsaExport_ContractSummary @msa_tam_post_proposal_ids
*/
CREATE PROCEDURE [dbo].[usp_PCS_MsaExport_ContractSummary]
	@msa_tam_post_proposal_ids UniqueIdTable READONLY
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	-- delivery by proposal/demo
	SELECT
		tppm.tam_post_id,
		tp.title,
		tppm.posting_plan_proposal_id,
		pa.ordinal,
		pa.audience_id,
		a.code,
		SUM(
			CASE p.is_equivalized 
				WHEN 0 THEN  
					pda.rating * (pda.us_universe * pd.universal_scaling_factor)/1000.0 
				ELSE
					pda.rating * ((pda.us_universe * pd.universal_scaling_factor)/1000.0) * sl.delivery_multiplier 
			END
			*
			pd.num_spots
		) 'contracted_delivery'
	FROM
		@msa_tam_post_proposal_ids mttp
		JOIN tam_post_proposals tppm ON tppm.id=mttp.id
		JOIN tam_posts tp ON tp.id=tppm.tam_post_id
		JOIN proposals p ON p.id=tppm.posting_plan_proposal_id
		JOIN proposal_audiences pa ON pa.proposal_id=tppm.posting_plan_proposal_id
			AND pa.ordinal BETWEEN 0 AND 5
		JOIN proposal_details pd ON pd.proposal_id=tppm.posting_plan_proposal_id
		JOIN spot_lengths sl ON sl.id=pd.spot_length_id
		JOIN proposal_detail_audiences pda ON pda.proposal_detail_id=pd.id
			AND pda.audience_id=pa.audience_id
		JOIN audiences a ON a.id=pa.audience_id
	GROUP BY
		tppm.tam_post_id,
		tp.title,
		tppm.posting_plan_proposal_id,
		pa.ordinal,
		pa.audience_id,
		a.code
END