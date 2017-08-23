-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/15/2015
-- Description:	Used to populate MSA Export file "contract_demos.txt"
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_MsaExport_ContractDemos]
	@msa_tam_post_proposal_ids UniqueIdTable READONLY
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    SELECT
		pa.audience_id 'demo_id',
		tp.id 'con_id',
		pa.ordinal,
		a.range_start 'demo_start_age',
		a.range_end 'demo_end_age',
		a.sub_category_code 'demo_type'
	FROM
		@msa_tam_post_proposal_ids mttp
		JOIN tam_post_proposals tpp ON tpp.id=mttp.id
		JOIN tam_posts tp ON tp.id=tpp.tam_post_id
		JOIN proposal_audiences pa ON pa.proposal_id=tpp.posting_plan_proposal_id
		JOIN audiences a ON a.id=pa.audience_id
	WHERE
		pa.ordinal BETWEEN 1 AND 5
	ORDER BY
		tp.id,
		pa.ordinal
END