CREATE PROCEDURE [dbo].[usp_ARSLoader_PreCrunchNielsenBroadcast]
	@media_month_id INT,
	@categoryRating varchar(15)
AS
BEGIN
	-- Local variables
	declare
		@idCategoryRating as int,
		@month as varchar(5),
		@textTimestamp as varchar(63);

	BEGIN TRY
		SET NOCOUNT ON; -- Used for performance and making sure we don't send back unneeded information.

		--Get rating ID
		select
			@idCategoryRating = id
		from
			rating_categories
		where
			code = @categoryRating;

		select
			@month = media_month
		from
			media_months
		where
			id = @media_month_id;
			
		set @textTimestamp = convert(varchar, getdate(), 121);
		raiserror(
			'%i|%s|%s - 1. Precrunching universes...', 
			0, 
			1, 
			@media_month_id,
			@categoryRating,
			@textTimestamp) with nowait;
		exec dbo.usp_ARSLoader_PreCrunchUniversesForNielsenBroadcast @month, @categoryRating;

		set @textTimestamp = convert(varchar, getdate(), 121);
		raiserror(
			'%i|%s|%s - 2. Precrunching ratings...', 
			0, 
			1,
			@media_month_id,
			@categoryRating, 
			@textTimestamp) with nowait;
		exec dbo.usp_ARSLoader_PreCrunchRatings @month, @categoryRating;
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

