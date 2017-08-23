-- =============================================
-- Author:		Nicholas Kheynis
-- Create date: 2/07/2014
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_BS_DeleteBroadcastFile]
	@broadcast_affidavit_id INT
AS
BEGIN 
	CREATE TABLE #MediaMonths (MediaMonthId INT)
	INSERT INTO #MediaMonths
		SELECT 
			mm.Id
		FROM 
			broadcast_affidavit_files daf (NOLOCK)
			JOIN media_months mm (NOLOCK) ON mm.start_date <= daf.end_date and mm.end_date >= daf.start_date
		WHERE 
			daf.id = @broadcast_affidavit_id
		
	BEGIN TRY
		BEGIN TRAN
		DELETE FROM 
			broadcast_post_details
		WHERE 
			broadcast_affidavit_file_id=@broadcast_affidavit_id;
			
		DELETE FROM  
			broadcast_posts
		WHERE 
			broadcast_affidavit_file_id=@broadcast_affidavit_id;
			
		DELETE FROM 
			broadcast_affidavits 
		WHERE 
			media_month_id in (SELECT MediaMonthId FROM #MediaMonths) 
			AND broadcast_affidavit_file_id=@broadcast_affidavit_id;
			
		DELETE FROM 
			broadcast_affidavit_files 
		WHERE 
			id=@broadcast_affidavit_id;
			
		COMMIT TRAN
	END TRY
	BEGIN CATCH
		ROLLBACK TRAN
	END CATCH
END
