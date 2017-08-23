
CREATE PROCEDURE usp_TamHelp_DeletePost
(
	@post_id int
)
AS BEGIN

BEGIN TRY

IF NOT EXISTS 
	(
		Select *
		from tam_post_proposals 
		where tam_post_id = @post_id
		and post_completed IS NULL
		AND aggregation_completed IS NULL
	)
	BEGIN
		RAISERROR('The post cannot be deleted because a posting plan containing the post has already been posted, or the post_id does not exist.', --message
		 16, --Severity
		 1) --State
	END
	BEGIN TRANSACTION
		DELETE FROM tam_post_material_substitutions WHERE tam_post_id = @post_id
		DELETE FROM tam_post_proposals WHERE tam_post_id = @post_id
		DELETE FROM tam_post_dayparts WHERE tam_post_id = @post_id
		DELETE FROM tam_post_report_options WHERE tam_post_id = @post_id
		DELETE FROM tam_posts where id = @post_id
	COMMIT TRANSACTION
END TRY
BEGIN CATCH
	ROLLBACK TRANSACTION
	DECLARE @ErrorMessage NVARCHAR(4000);
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;

    SELECT 
        @ErrorMessage = ERROR_MESSAGE(),
        @ErrorSeverity = ERROR_SEVERITY(),
        @ErrorState = ERROR_STATE();

    --need to RAISERROR AGAIN to report it back to the user
    RAISERROR (@ErrorMessage, -- Message text.
               @ErrorSeverity, -- Severity.
               @ErrorState -- State.
               );
END CATCH;

END