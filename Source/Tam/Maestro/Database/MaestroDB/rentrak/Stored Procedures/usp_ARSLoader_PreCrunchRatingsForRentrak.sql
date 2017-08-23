CREATE PROCEDURE [rentrak].[usp_ARSLoader_PreCrunchRatingsForRentrak]
	@idMediaMonth AS INT,
	@idRatingCategory AS INT
AS
BEGIN
	DECLARE
		@textTimestamp AS VARCHAR(63),
		@textRows AS VARCHAR(63),
		@rows AS INT,
		@codeTimestampFormat AS INT;

	-- Enable dirty reads
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; 
	-- Don't auto report rows affected.
	SET NOCOUNT ON; 
	-- Make Monday the first day of the week.
	SET DATEFIRST 1;
	-- Use ODBC canonical (WITH milliseconds) format for timestamps <yyyy-mm-dd hh:mi:ss.mmm(24h)>
	SET @codeTimestampFormat = 121;

	BEGIN TRY
		BEGIN TRANSACTION;

		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), @codeTimestampFormat);
		raiserror(
			'%s - Processing actual ratings data for %d''s %d book...', 
			0, 
			1, 
			@textTimestamp,
			@idRatingCategory,
			@idMediaMonth) WITH nowait;

		DECLARE @dayparts TABLE(daypart_id INT NOT NULL,start_time INT NOT NULL, end_time INT NOT NULL)
		INSERT INTO @dayparts(daypart_id,start_time,end_time)
			SELECT DISTINCT
				dpm.daypart_id,
				ts.start_time,
				ts.end_time
			FROM 
				daypart_maps dpm
				JOIN timespans ts ON ts.id = dpm.daypart_id
			WHERE
				dpm.map_set IN ('Ratings', 'FM_nDyptNum');

		SET @textTimestamp = convert(varchar, getdate(), @codeTimestampFormat);
		raiserror(
			'%s - Isolating relevant rentrak viewership records...', 
			0, 
			1, 
			@textTimestamp) WITH nowait;

		IF OBJECT_ID(N'#tmp_rentrak_viewership') IS NOT NULL
			DROP TABLE #tmp_rentrak_viewership;

		CREATE TABLE #tmp_rentrak_viewership (
			[demographic_number] [INT] NOT NULL,
			[rentrak_id] [INT] NOT NULL,
			[rating_date] [date] NOT NULL,
			[rating_day_ordinal] [INT] NOT NULL,
			[start_time] [INT] NOT NULL,
			[end_time] [INT] NOT NULL,
			[usage] [FLOAT] NOT NULL,
			CONSTRAINT [PK_p_ratings] PRIMARY KEY CLUSTERED 
			(
				[demographic_number] ASC,
				[rentrak_id] ASC,
				[rating_date] ASC,
				[start_time] ASC,
				[end_time] ASC
			)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
		);
		CREATE NONCLUSTERED INDEX [IX_tmp_rentrak_viewership] ON #tmp_rentrak_viewership ([rating_day_ordinal],[start_time]) 
			INCLUDE ([rentrak_id],[demographic_number],[usage])

		INSERT INTO
			#tmp_rentrak_viewership(
				[demographic_number],
				[rentrak_id],
				[rating_date],
				[rating_day_ordinal],
				[start_time],
				[end_time],
				[usage]
			)
			SELECT
				vwrs.demographic_number,
				vwrs.rentrak_id,			
				CAST(vwrs.time AS DATE) [rating_date],
				datepart(weekday, vwrs.time) [rating_day_ordinal],
				datediff(second, cast(vwrs.time AS date), vwrs.time) [start_time],
				datediff(second, cast(vwrs.time AS date), vwrs.time) + ((60*15)-1) [end_time],
				vwrs.average_audience [usage]
			FROM
				rentrak.rentrak.viewership vwrs
			WHERE
				vwrs.media_month_id = @idMediaMonth
				AND
				vwrs.demographic_number <> 'ALL'
			ORDER BY
				[rentrak_id] ASC,
				[demographic_number] ASC,
				[rating_date] ASC,
				[start_time] ASC;

		SET @rows = @@rowcount;
		SET @textRows = LEFT(
			convert(varchar, cast(@rows AS money), 1), 
			len(convert(varchar, cast(@rows AS money), 1))-3);
		SET @textTimestamp = convert(varchar, getdate(), 121);
		raiserror(
			'%s - %s rows isolated FROM rentrak.rentrak.viewership for media month id %s.', 
			0, 
			1, 
			@textTimestamp,
			@textRows,
			@idMediaMonth) WITH nowait;

		SET @textTimestamp = convert(varchar, getdate(), @codeTimestampFormat);
		raiserror(
			'%s - Deleting old records for media month id %d AND rating category id %d FROM rentrak ratings table...', 
			0, 
			1, 
			@textTimestamp,
			@idMediaMonth,
			@idRatingCategory) WITH nowait;

		DELETE FROM 
			ratings 
		WHERE 
			rating_category_id = @idRatingCategory
			AND base_media_month_id = @idMediaMonth;

		SET @rows = @@rowcount;
		SET @textRows = LEFT(
			convert(varchar, cast(@rows AS money), 1), 
			len(convert(varchar, cast(@rows AS money), 1))-3);
		SET @textTimestamp = convert(varchar, getdate(), @codeTimestampFormat);
		raiserror(
			'%s - %s rows deleted FROM ratings.', 
			0, 
			1, 
			@textTimestamp,
			@textRows) WITH nowait;

		SET @textTimestamp = convert(varchar, getdate(), @codeTimestampFormat);
		raiserror(
			'%s - Filling ratings...', 
			0, 
			1, 
			@textTimestamp) WITH nowait;

		SET @textTimestamp = convert(varchar, getdate(), @codeTimestampFormat);
		raiserror(
			'%s - Adding ratings for non-overnight dayparts...', 
			0, 
			1, 
			@textTimestamp) WITH nowait;

		INSERT INTO ratings (
			rating_category_id,
			base_media_month_id,
			forecast_media_month_id,
			nielsen_network_id,
			audience_id,
			daypart_id,
			audience_usage,
			tv_usage
		)
			SELECT
				@idRatingCategory [rating_category_id],
				@idMediaMonth [base_media_month_id],
				@idMediaMonth [forecast_media_month_id],
				nnm.nielsen_network_id [nielsen_network_id],
				am.audience_id [audience_id],
				dp.id [daypart_id],				
				AVG(mr.usage) [audience_usage],
				-4000 [tv_usage]				
			FROM
				@dayparts dpt 
				JOIN dayparts dp ON dpt.daypart_id = dp.id
				JOIN daypart_days dp_d ON dp.id = dp_d.daypart_id
				JOIN days d ON d.id = dp_d.day_id
				JOIN #tmp_rentrak_viewership mr ON mr.start_time BETWEEN dpt.start_time AND dpt.end_time
					AND d.ordinal = mr.rating_day_ordinal
				JOIN nielsen_network_maps nnm ON nnm.map_set = 'Rentrak' 
					AND nnm.map_value = CAST(mr.rentrak_id AS VARCHAR)
				JOIN audience_maps am ON am.map_set = 'Rentrak' 
					AND am.map_value = CAST(mr.demographic_number AS VARCHAR)
			WHERE
				dpt.start_time <= dpt.end_time
			GROUP BY
				dp.id,
				nnm.nielsen_network_id,
				am.audience_id
			ORDER BY
				[daypart_id],
				[nielsen_network_id],
				[audience_id];

		SET @rows = @@rowcount;
		SET @textRows = LEFT(
			convert(varchar, cast(@rows AS money), 1), 
			len(convert(varchar, cast(@rows AS money), 1))-3);
		SET @textTimestamp = convert(varchar, getdate(), 121);
		raiserror(
			'%s - %s rows added to ratings for non-overnignt dayparts.', 
			0, 
			1, 
			@textTimestamp,
			@textRows) WITH nowait;

		SET @textTimestamp = convert(varchar, getdate(), @codeTimestampFormat);
		raiserror(
			'%s - Adding ratings for overnight dayparts...', 
			0, 
			1, 
			@textTimestamp) WITH nowait;

		INSERT INTO ratings (
			rating_category_id,
			base_media_month_id,
			forecast_media_month_id,
			nielsen_network_id,
			audience_id,
			daypart_id,
			audience_usage,
			tv_usage
		)
			SELECT
				@idRatingCategory [rating_category_id],
				@idMediaMonth [base_media_month_id],
				@idMediaMonth [forecast_media_month_id],
				nnm.nielsen_network_id [nielsen_network_id],
				am.audience_id [audience_id],
				dp.id [daypart_id],
				AVG(mr.usage) [audience_usage],
				-4000 [tv_usage]				
			FROM
				@dayparts dpt 
				JOIN dayparts dp ON dpt.daypart_id = dp.id
				JOIN daypart_days dp_d ON dp.id = dp_d.daypart_id
				JOIN days d ON d.id = dp_d.day_id
				JOIN #tmp_rentrak_viewership mr ON d.ordinal = mr.rating_day_ordinal
					AND
					(
						mr.start_time BETWEEN dpt.start_time AND 86340
						or
						mr.start_time BETWEEN 0 AND dpt.end_time
					) 
				JOIN nielsen_network_maps nnm
					ON nnm.map_set = 'Rentrak' AND nnm.map_value = CAST(mr.rentrak_id AS VARCHAR)
				JOIN audience_maps am
					ON am.map_set = 'Rentrak' AND am.map_value = CAST(mr.demographic_number AS VARCHAR)
			WHERE
				dpt.start_time > dpt.end_time
			GROUP BY
				dp.id,
				nnm.nielsen_network_id,
				am.audience_id
			ORDER BY
				[daypart_id],
				[nielsen_network_id],
				[audience_id];

			SET @rows = @@rowcount;
			SET @textRows = LEFT(
				convert(varchar, cast(@rows AS money), 1), 
				len(convert(varchar, cast(@rows AS money), 1))-3);
			SET @textTimestamp = convert(varchar, getdate(), 121);
			raiserror(
				'%s - %s rows added to ratings for overnignt dayparts.', 
				0, 
				1, 
				@textTimestamp,
				@textRows) WITH nowait;

		COMMIT TRANSACTION;

		SET @textTimestamp = convert(varchar, getdate(), @codeTimestampFormat);
		raiserror(
			'%s - Done processing actual ratings data for rating category id %d''s %d book.', 
			0, 
			1, 
			@textTimestamp,
			@idRatingCategory,
			@idMediaMonth) WITH nowait;

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
		
		IF (XACT_STATE()) = -1-- If -1, the transaction IS uncommittable
			BEGIN
			PRINT
				N'The transaction IS IN an uncommittable state. ' +
				'Rolling back transaction.'
			ROLLBACK TRANSACTION;
			END;
		ELSE IF (XACT_STATE()) = 1-- If 1, the transaction IS committable.
			BEGIN
			PRINT
				N'The transaction IS committable. ' +
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
