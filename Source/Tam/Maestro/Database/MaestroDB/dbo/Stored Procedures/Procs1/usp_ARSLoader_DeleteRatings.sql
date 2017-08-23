CREATE procedure [dbo].[usp_ARSLoader_DeleteRatings]
	@StartDate datetime,
	@EndDate datetime,
	@RatingCategoryID int
AS
BEGIN
	BEGIN TRY
		BEGIN TRANSACTION;

		DECLARE 
			@TempStartDate DATETIME,
			@TempEndDate DATETIME;
			
		SET @TempStartDate = dateadd(ss, 21600, @StartDate);
		SET @TempEndDate = dateadd(ss, 21599, dateadd(dd,1,@EndDate));
		
		-- flight could cross media months as the end date goes into the next month
		CREATE TABLE #media_months (media_month_id INT)
		INSERT INTO #media_months
			SELECT DISTINCT mm.id FROM dbo.media_months mm (NOLOCK) WHERE mm.start_date <= @TempEndDate AND mm.end_date >= @TempStartDate

		CREATE TABLE #mit_rating_ids (id INT not null, PRIMARY KEY CLUSTERED(id ASC));
		INSERT INTO #mit_rating_ids
			SELECT 
				mr.id 
			FROM 
				dbo.mit_ratings mr (NOLOCK)
				JOIN #media_months mm ON mm.media_month_id=mr.media_month_id
			WHERE 
				mr.rating_category_id = @RatingCategoryID
				AND DATEADD(ss, mr.start_time, CAST(rating_date AS DATETIME)) BETWEEN @TempStartDate and @TempEndDate;

		DELETE FROM 
			mit_person_audiences
		FROM
			mit_person_audiences mpa
			JOIN #media_months mm ON mm.media_month_id=mpa.media_month_id
			JOIN #mit_rating_ids mrids ON mrids.id=mpa.mit_rating_id

		DELETE FROM 
			mit_tv_audiences
		FROM
			mit_tv_audiences mta
			JOIN #media_months mm ON mm.media_month_id=mta.media_month_id
			JOIN #mit_rating_ids mrids ON mrids.id=mta.mit_rating_id

		DELETE FROM 
			mit_ratings
		FROM
			mit_ratings mr
			JOIN #media_months mm ON mm.media_month_id=mr.media_month_id
			JOIN #mit_rating_ids mrids ON mrids.id=mr.id
		
		DROP TABLE #media_months;
		DROP TABLE #mit_rating_ids;

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
