-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/13/2014 05:19:04 PM
-- Description:	Auto-generated method to update a msa_deliveries record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_msa_deliveries_update]
	@media_month_id SMALLINT,
	@tam_post_proposal_id INT,
	@tam_post_affidavit_id BIGINT,
	@audience_id INT,
	@msa_delivery_files_id INT,
	@is_equivalized BIT,
	@delivery FLOAT
AS
BEGIN
	UPDATE
		[dbo].[msa_deliveries]
	SET
		[msa_delivery_files_id]=@msa_delivery_files_id,
		[is_equivalized]=@is_equivalized,
		[delivery]=@delivery
	WHERE
		[media_month_id]=@media_month_id
		AND [tam_post_proposal_id]=@tam_post_proposal_id
		AND [tam_post_affidavit_id]=@tam_post_affidavit_id
		AND [audience_id]=@audience_id
END
