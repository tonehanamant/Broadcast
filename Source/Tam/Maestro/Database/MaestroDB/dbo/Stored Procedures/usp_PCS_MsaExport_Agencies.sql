
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/15/2015
-- Description:	Used to populate MSA Export file "agencies.txt"
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_MsaExport_Agencies]
	@msa_tam_post_proposal_ids UniqueIdTable READONLY
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	SELECT
		p.agency_company_id
	FROM
		tam_post_proposals tpp
		JOIN proposals p ON p.id=tpp.posting_plan_proposal_id
	WHERE
		tpp.id IN (
			SELECT id FROM @msa_tam_post_proposal_ids
		)
	GROUP BY
		p.agency_company_id
	ORDER BY
		p.agency_company_id
END