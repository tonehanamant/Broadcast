-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/13/2014 05:19:04 PM
-- Description:	Auto-generated method to insert a msa_deliveries record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_msa_deliveries_insert]
	@media_month_id SMALLINT,
	@tam_post_proposal_id INT,
	@tam_post_affidavit_id BIGINT,
	@audience_id INT,
	@msa_delivery_files_id INT,
	@is_equivalized BIT,
	@delivery FLOAT
AS
BEGIN
	INSERT INTO [dbo].[msa_deliveries]
	(
		[media_month_id],
		[tam_post_proposal_id],
		[tam_post_affidavit_id],
		[audience_id],
		[msa_delivery_files_id],
		[is_equivalized],
		[delivery]
	)
	VALUES
	(
		@media_month_id,
		@tam_post_proposal_id,
		@tam_post_affidavit_id,
		@audience_id,
		@msa_delivery_files_id,
		@is_equivalized,
		@delivery
	)
END
