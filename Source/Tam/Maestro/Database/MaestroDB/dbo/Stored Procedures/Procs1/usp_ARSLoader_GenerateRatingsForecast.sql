CREATE PROCEDURE [dbo].[usp_ARSLoader_GenerateRatingsForecast]
	@media_month_id INT,
	@categoryRating varchar(15)
AS
BEGIN
	-- Local variables
	declare
		@date as datetime,
		@idCategoryRating as int,
		@month as varchar(5),
		@textTimestamp as varchar(63);

	BEGIN TRY
--		BEGIN TRANSACTION;
		SET NOCOUNT ON; -- Used for performance and making sure we don't send back unneeded information.

		--Get rating ID
		select
			@idCategoryRating = id
		from
			rating_categories
		where
			code = @categoryRating;

		select
			@month = media_month,
			@date = dateadd(day,15,start_date)
		from
			media_months
		where
			id = @media_month_id;

		set @textTimestamp = convert(varchar, getdate(), 121);
		raiserror(
			'%i|%s|%s - Forecasting ratings from %s actuals...', 
			0, 
			1,
			@media_month_id,
			@categoryRating,
			@textTimestamp,
			@month) with nowait;

		set @textTimestamp = convert(varchar, getdate(), 121);
		raiserror(
			'%i|%s|%s - 1. Calculating base ratings...', 
			0, 
			1, 
			@media_month_id,
			@categoryRating,
			@textTimestamp) with nowait;
		exec dbo.usp_update_base_ratings @date, 3, @categoryRating;

		set @textTimestamp = convert(varchar, getdate(), 121);
		raiserror(
			'%i|%s|%s - 2. Calculating general ratings...', 
			0, 
			1, 
			@media_month_id,
			@categoryRating,
			@textTimestamp) with nowait;
		exec dbo.usp_update_general_ratings @date, @categoryRating;

		set @textTimestamp = convert(varchar, getdate(), 121);
		raiserror(
			'%i|%s|%s - 3. Calculating general base ratings...', 
			0, 
			1, 
			@media_month_id,
			@categoryRating,
			@textTimestamp) with nowait;
		exec dbo.usp_update_general_base_ratings @date, 3, @categoryRating;

		set @textTimestamp = convert(varchar, getdate(), 121);
		raiserror(
			'%i|%s|%s - 4. Forecasting universes...', 
			0, 
			1, 
			@media_month_id,
			@categoryRating,
			@textTimestamp) with nowait;
		exec dbo.usp_ARSLoader_ForecastingUniverses @media_month_id, @idCategoryRating;

		set @textTimestamp = convert(varchar, getdate(), 121);
		raiserror(
			'%i|%s|%s - 5. Forecasting ratings...', 
			0, 
			1, 
			@media_month_id,
			@categoryRating,
			@textTimestamp) with nowait;
		exec dbo.usp_ARSLoader_ForecastingRatings @month, @categoryRating;

		set @textTimestamp = convert(varchar, getdate(), 121);
		raiserror(
			'%i|%s|%s - 6. Forecasting ratings for second year...', 
			0, 
			1, 
			@media_month_id,
			@categoryRating,
			@textTimestamp) with nowait;
		exec dbo.usp_ARSLoader_ForecastRatingsMoreThan1Year @media_month_id, @idCategoryRating;

		set @textTimestamp = convert(varchar, getdate(), 121);
		raiserror(
			'%i|%s|%s - Forecasting ratings from %s actuals is done.', 
			0, 
			1, 
			@media_month_id,
			@categoryRating,
			@textTimestamp,
			@month) with nowait;
--		COMMIT TRANSACTION;   
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

