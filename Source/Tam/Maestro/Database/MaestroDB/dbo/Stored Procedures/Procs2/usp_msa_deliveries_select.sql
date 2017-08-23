﻿-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/13/2014 05:19:04 PM
-- Description:	Auto-generated method to delete or potentionally disable a msa_deliveries record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_msa_deliveries_select]
	@media_month_id SMALLINT,
	@tam_post_proposal_id INT,
	@tam_post_affidavit_id BIGINT,
	@audience_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[msa_deliveries].*
	FROM
		[dbo].[msa_deliveries] WITH(NOLOCK)
	WHERE
		[media_month_id]=@media_month_id
		AND [tam_post_proposal_id]=@tam_post_proposal_id
		AND [tam_post_affidavit_id]=@tam_post_affidavit_id
		AND [audience_id]=@audience_id
END
