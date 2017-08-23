-- =============================================
-- Author:		David Sisson
-- Create date: 5/31/09
-- Description:	Returns all component audiences.
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARS_GetComponentAudiences] 
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		SELECT
			a.*
		FROM
			audiences a (NOLOCK)
		WHERE
			a.category_code=0
			AND a.custom = 0;
	END TRY
	BEGIN CATCH
		DECLARE @ErrorMessage NVARCHAR(4000);
		DECLARE @ErrorSeverity INT;
		DECLARE @ErrorState INT;

		SELECT 
			@ErrorMessage = ERROR_MESSAGE(),
			@ErrorSeverity = ERROR_SEVERITY(),
			@ErrorState = ERROR_STATE();

		-- Use RAISERROR inside the CATCH block to return error
		-- information about the original error that caused
		-- execution to jump to the CATCH block.
		RAISERROR (@ErrorMessage, -- Message text.
				   @ErrorSeverity, -- Severity.
				   @ErrorState -- State.
				   );
	END CATCH;
END
