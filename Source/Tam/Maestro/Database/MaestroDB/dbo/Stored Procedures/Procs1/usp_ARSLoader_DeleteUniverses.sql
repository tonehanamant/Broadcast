-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Modified:	10/22/2013 - Stephen DeFusco, added media_month_id and NOLOCK.
--				05/20/2015 - Stephen DeFusco, Modified to support deletion of flights crossing multiple media months, previously you could only delete one.
-- Description:	
-- =============================================
CREATE procedure [dbo].[usp_ARSLoader_DeleteUniverses]
	@StartDate datetime,
	@EndDate datetime,
	@RatingCategoryID int
AS
BEGIN
	BEGIN TRY
		DECLARE @media_month_ids TABLE(id INT); 
		INSERT INTO @media_month_ids
			SELECT mm.id FROM dbo.media_months mm (NOLOCK) WHERE mm.start_date <= @EndDate AND mm.end_date >= @StartDate;
		
		BEGIN TRANSACTION;
		
		DECLARE @mit_universe_ids TABLE(id INT);

		INSERT INTO @mit_universe_ids
			SELECT
				mu.id 
			FROM 
				mit_universes mu (NOLOCK)
			WHERE
				mu.media_month_id IN (
					SELECT id FROM @media_month_ids
				)
				AND mu.rating_category_id = @RatingCategoryID
				AND mu.start_date <= @EndDate AND mu.end_date >= @StartDate;

		DELETE FROM 
			mit_universe_audiences 
		WHERE 
			media_month_id IN (
				SELECT id FROM @media_month_ids
			)
			AND mit_universe_id IN (
				SELECT id  FROM @mit_universe_ids
			);
			
		DELETE FROM 
			mit_universes 
		WHERE 
			media_month_id IN (
				SELECT id FROM @media_month_ids
			)
			AND id IN (
				select id  FROM @mit_universe_ids
			);
						
		COMMIT TRANSACTION;   
	END TRY
	BEGIN CATCH
		DECLARE @ErrorNumber INT;
		DECLARE @ErrorSeverity INT;
		DECLARE @ErrorState INT;
		DECLARE @ErrorProcedure NVARCHAR(200) ;
		DECLARE @ErrorLine INT;
		DECLARE @ErrorMessage NVARCHAR(4000);
		SELECT 
			@ErrorNumber = ERROR_NUMBER(),
			@ErrorSeverity = ERROR_SEVERITY(),
			@ErrorState = ERROR_STATE(),
			@ErrorProcedure = isnull(ERROR_PROCEDURE(), 'N/A'),
			@ErrorLine = ERROR_LINE(),
			@ErrorMessage = N'Error %d, Level %d, State %d, Procedure %s, Line %d, ' + 
				'Message: '+ ERROR_MESSAGE();
		
		IF (XACT_STATE()) = -1-- If -1, the transaction is uncommittable
			BEGIN
			PRINT
				N'The transaction is in an uncommittable state. ' +
				'Rolling back transaction.'
			ROLLBACK TRANSACTION;
			END;
		ELSE IF (XACT_STATE()) = 1-- If 1, the transaction is committable.
			BEGIN
			PRINT
				N'The transaction is committable. ' +
				'Committing transaction.'
			COMMIT TRANSACTION;   
			END;

		RAISERROR(
			@ErrorMessage, 
			@ErrorSeverity, 
			1,               
			@ErrorNumber,    -- parameter: original error number.
			@ErrorSeverity,  -- parameter: original error severity.
			@ErrorState,     -- parameter: original error state.
			@ErrorProcedure, -- parameter: original error procedure name.
			@ErrorLine       -- parameter: original error line number.
		);
	END CATCH;
END
