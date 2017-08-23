CREATE PROC [wb].[usp_delete_wb_shedules_for_week] (
	@WeekStartDate		[varchar](127), 	--First parameter
	@RegionCode			[varchar](127)
)
AS
BEGIN
	BEGIN TRY
		SET NOCOUNT ON;
		
		--Local constants
		-- NONE
		
		--Local variables
		DECLARE
			@rows AS INT,
			@textRows AS VARCHAR(63),
			@textMessage AS VARCHAR(255),
			@textTimestamp AS VARCHAR(63),
			@dateWeekStart AS DATETIME,
			@idMediaWeek AS INT;
			
		-- Parameter validation
		--Validate first parameter, WeekStartDate...
		IF ISDATE(@WeekStartDate) <> 1
			BEGIN
			SET @textMessage = 
				'Invalid parameter. The second parameter, WeekStartDate, ' + 
				'which has a value of ''' + ISNULL(@WeekStartDate, '<NULL>') + 
				''', is not a date.';
			SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
			RAISERROR(
				'%s - %s', 
				15, 
				102,
				@textTimestamp,
				@textMessage
			);
			END;
		SET @dateWeekStart = CAST(@WeekStartDate AS DATETIME);
		SELECT
			@idMediaWeek = id
		FROM
			dbo.media_weeks
		WHERE
			dbo.media_weeks.start_date = @dateWeekStart;
		
		IF @idMediaWeek IS NULL
			BEGIN
			SET @textMessage = 
				'Invalid parameter. The second parameter, WeekStartDate, ' + 
				'which has a value of ''' + 
				CONVERT(VARCHAR, @dateWeekStart, 111) + ''',  must fall on a ' + 
				'Monday. WeekStartDate falls on a ' + DATENAME(weekday, @dateWeekStart) + '.';
			SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
			RAISERROR(
				'%s - %s', 
				15, 
				102,
				@textTimestamp,
				@textMessage
			);
			END;

		BEGIN TRANSACTION;
		
		DELETE
		FROM
			wb.wb_schedules
		WHERE
			@idMediaWeek = media_week_id
				AND @RegionCode = region_code;

		SET @rows = @@rowcount;
		SET @textRows = LEFT(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1), LEN(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1)) - 3);
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR(
						'%s - %s records deleted from wb.wb_schedules table.', 
						0, 
						1, 
						@textTimestamp,
						@textRows) WITH NOWAIT;

		COMMIT TRANSACTION;   
	END TRY
	BEGIN CATCH
        -- Re-raise the original error.
        EXEC wb.usp_RethrowError;
	END CATCH;
END;
