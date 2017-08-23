-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/21/2015
-- Description:	Used to mark the tam_post_proposal records for MSA as received (or completed).
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_MsaImport_MarkCompleted]
	@msa_tam_post_proposal_ids UniqueIdTable READONLY
AS
BEGIN
	SET NOCOUNT ON;

    UPDATE
		tam_post_proposals
	SET
		msa_status_code = 2 -- COMPLETED
	WHERE
		id IN (
			SELECT id FROM @msa_tam_post_proposal_ids
		)
END