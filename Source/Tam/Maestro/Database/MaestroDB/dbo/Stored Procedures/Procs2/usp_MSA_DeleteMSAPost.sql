-- =============================================
-- Author:		Nicholas Kheynis
-- Create date: 4/11/2014
-- Description:	
-- =============================================
-- nsi.usp_NSI_GetPostableMediaMonths
CREATE PROCEDURE [dbo].[usp_MSA_DeleteMSAPost]
	@media_month_id INT,
	@file_name VARCHAR(255)

AS
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		BEGIN TRAN
		
		-- 1) msa_post_weekly_details
		DELETE 
			dbo.msa_post_weekly_details 
		FROM 
			dbo.msa_post_weekly_details
			JOIN dbo.msa_posts (NOLOCK) ON dbo.msa_posts.id = dbo.msa_post_weekly_details.msa_post_id
		WHERE
			msa_posts.media_month_id = @media_month_id 
			AND msa_posts.file_name = @file_name;
		
		-- 2) msa_post_daypart_details
		DELETE 
			dbo.msa_post_daypart_details
		FROM 
			dbo.msa_post_daypart_details
			JOIN dbo.msa_posts (NOLOCK) ON msa_posts.id = msa_post_daypart_details.msa_post_id
		WHERE
			msa_posts.media_month_id = @media_month_id 
			AND msa_posts.file_name = @file_name;
		
		-- 3) msa_post_isci_details	
		DELETE
			dbo.msa_post_isci_details
		FROM 
			dbo.msa_post_isci_details
			JOIN dbo.msa_posts (NOLOCK) ON msa_posts.id = msa_post_isci_details.msa_post_id
		WHERE
			msa_posts.media_month_id = @media_month_id 
			AND msa_posts.file_name = @file_name;
			
		-- 4) msa_posts	
		DELETE FROM
			dbo.msa_posts
		WHERE
			msa_posts.media_month_id = @media_month_id 
			AND msa_posts.file_name = @file_name;
			
		COMMIT TRAN
	END TRY
	BEGIN CATCH
		ROLLBACK TRAN
	END CATCH
END
