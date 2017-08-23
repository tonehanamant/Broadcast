-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 8/5/2015
-- Description:	Used to mark the tam_post_proposal records for MSA as exported.
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_MsaExport_MarkExported]
	@msa_tam_post_proposal_ids UniqueIdTable READONLY
AS
BEGIN
	SET NOCOUNT ON;

    UPDATE
		tam_post_proposals
	SET
		msa_status_code = 1, -- PENDING
		date_exported_to_msa = GETDATE()
	WHERE
		id IN (
			SELECT id FROM @msa_tam_post_proposal_ids
		)
END