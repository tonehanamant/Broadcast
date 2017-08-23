-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/10/2015
-- Description:	Used to create PDF MSA Media Plans
-- =============================================
/*
	DECLARE @msa_tam_post_proposal_ids UniqueIdTable;
	INSERT INTO @msa_tam_post_proposal_ids SELECT 1003487
	INSERT INTO @msa_tam_post_proposal_ids SELECT 1003609
	EXEC usp_PCS_MsaExport_MediaPlanPdfs @msa_tam_post_proposal_ids
*/
CREATE PROCEDURE [dbo].[usp_PCS_MsaExport_MediaPlanPdfs]
	@msa_tam_post_proposal_ids UniqueIdTable READONLY
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    SELECT
		tpp.tam_post_id 'con_id',
		tpp.posting_plan_proposal_id,
		pa.audience_id
	FROM
		@msa_tam_post_proposal_ids mttp
		JOIN tam_post_proposals tpp ON tpp.id=mttp.id
		JOIN proposal_audiences pa ON pa.proposal_id=tpp.posting_plan_proposal_id
	WHERE
		pa.ordinal BETWEEN 1 AND 5
	ORDER BY
		tpp.tam_post_id,
		pa.audience_id
END