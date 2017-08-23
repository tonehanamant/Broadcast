CREATE PROC [wb].[usp_insert_wb_schedule] (
	@RegionCode			[varchar](127),		--First parameter
	@WeekStartDate		[varchar](127), 	--Second parameter
	@StartTime			[varchar](127), 	--Third parameter
	@EndTime			[varchar](127), 	--Fourth parameter
	@DayOfWeek			[varchar](127), 	--Fifth parameter
	@AgencyName			[varchar](127), 	--Sixth parameter
	@OrderTitle			[varchar](127), 	--Seventh parameter
	@Status				[varchar](127), 	--Eighth parameter
	@NetRate			[varchar](127), 	--Ninth parameter
	@AgencyPercentage	[varchar](127), 	--Tenth parameter
	@GrossRate			[varchar](127), 	--Eleventh parameter
	@TapeID				[varchar](127), 	--Twelfth parameter
	@PhoneNumber		[varchar](127), 	--Thirteenth parameter
	@SalesPerson		[varchar](127), 	--Fourteenth parameter
	@RateCardRate		[varchar](127)		--Fifteenth parameter
)
AS
BEGIN
	BEGIN TRY
		SET NOCOUNT ON;

		--Local constants
		DECLARE
			@timeINVALID AS INTEGER,
			@timeMINVALID AS INTEGER,
			@timeMAXVALID AS INTEGER;
			
		SET @timeINVALID = -1;
		SET @timeMINVALID = 0;
		SET @timeMAXVALID = (24 * 60 * 60) - 1;
		
		--Local variables
		DECLARE
			@textMessage AS VARCHAR(255),
			@textTimestamp AS VARCHAR(63),
			@codeRegion AS VARCHAR(127),
			@dateWeekStart AS DATETIME,
			@idMediaWeek AS INT,
			@timeStart AS INT,
			@timeEnd AS INT,
			@idDay AS INT,
			@idWBAgency AS INT,
			@titleOrder AS VARCHAR(127),
			@rateNet AS MONEY,
			@codeStatus AS VARCHAR(127),
			@percentageAgency AS DECIMAL(10,9),
			@rateGross AS MONEY,
			@codeTape AS VARCHAR(127),
			@codePhoneNumber AS VARCHAR(127),
			@nameSalesPerson AS VARCHAR(127),
			@rateRatecard AS MONEY;
			
		-- Parameter validation
		--Validate first parameter, RegionCode...
		SET @codeRegion = LTRIM(RTRIM(ISNULL(@RegionCode, '')));
		IF LEN(@codeRegion) = 0
			BEGIN
			SET @textMessage = 
				'Invalid parameter. The first parameter, RegionCode, ' + 
				'is missing or blank. RegionCode must be a string with at ' +
				'least one non-whitespace character.';
			SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
			RAISERROR(
				'%s - %s', 
				15, 
				101,
				@textTimestamp,
				@textMessage
			);
			END;
		
		--Validate second parameter, WeekStartDate...
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
		
		--Validate third parameter, StartTime...
		IF ISNUMERIC(@StartTime) <> 1
			BEGIN
			SET @textMessage = 
				'Invalid parameter. The third parameter, StartTime, ' + 
				'which has a value of ''' + ISNULL(@StartTime, '<NULL>') + 
				''', is not an integer.';
			SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
			RAISERROR(
				'%s - %s', 
				15, 
				103,
				@textTimestamp,
				@textMessage
			);
			END;
		SET @timeStart = CAST(@StartTime AS INT);
		IF (@timeMINVALID > @timeStart) OR (@timeStart > @timeMAXVALID)
			BEGIN
			SET @textMessage = 
				'Invalid parameter. The third parameter, StartTime, ' + 
				'which has a value of ''' + ISNULL(@StartTime, '<NULL>') + 
				''', must have a value between %d and %d.';
			SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
			RAISERROR(
				'%s - %s', 
				15, 
				103,
				@textTimestamp,
				@textMessage,
				@timeStart,
				@timeEnd
			);
			END;

		--Validate fourth parameter, EndTime...
		IF ISNUMERIC(@EndTime) <> 1
			BEGIN
			SET @textMessage = 
				'Invalid parameter. The fourth parameter, EndTime, ' + 
				'which has a value of ''' + ISNULL(@EndTime, '<NULL>') + 
				''', is not an integer.';
			SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
			RAISERROR(
				'%s - %s', 
				15, 
				104,
				@textTimestamp,
				@textMessage
			);
			END;
		SET @timeEnd = CAST(@EndTime AS INT);
		IF (@timeMINVALID > @timeStart) OR (@timeStart > @timeMAXVALID)
			BEGIN
			SET @textMessage = 
				'Invalid parameter. The fourth parameter, EndTime, ' + 
				'which has a value of ''' + ISNULL(@EndTime, '<NULL>') + 
				''', must have a value between %d and %d.';
			SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
			RAISERROR(
				'%s - %s', 
				15, 
				104,
				@textTimestamp,
				@textMessage,
				@timeStart,
				@timeEnd
			);
			END;

		--Validate fifth parameter, DayOfWeek...
		SELECT
			@idDay = id
		FROM
			dbo.days
		WHERE
			dbo.days.name = @DayOfWeek;
		
		IF @idDay IS NULL
			BEGIN
			SET @textMessage = 
				'Invalid parameter. The fifth parameter, DayOfWeek, ' + 
				'which has a value of ''' + 
				ISNULL(@DayOfWeek, '<NULL>') + ''',  must have a value of ' +
				'a day of the week, Monday - Sunday.';
			SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
			RAISERROR(
				'%s - %s', 
				15, 
				105,
				@textTimestamp,
				@textMessage
			);
			END;
		
		--Validate sixth parameter, AgencyName...
		SELECT
			@idWBAgency = id
		FROM
			wb.wb_agencies
		WHERE
			wb.wb_agencies.code = LTRIM(RTRIM(@AgencyName));
		
		IF @idWBAgency IS NULL
			BEGIN
			SET @textMessage = 
				'Invalid parameter. The sixth parameter, AgencyName, ' + 
				'which has a value of ''' + 
				LTRIM(RTRIM(ISNULL(@AgencyName, '<NULL>'))) + ''',  was ' +
				'not found in Wize Buys agency list.';
			SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
			RAISERROR(
				'%s - %s', 
				15, 
				106,
				@textTimestamp,
				@textMessage
			);
			END;
		
		--Validate seventh parameter, OrderTitle...
		SET @titleOrder = LTRIM(RTRIM(ISNULL(@OrderTitle, '')));
		IF LEN(@titleOrder) = 0
			BEGIN
			SET @textMessage = 
				'Invalid parameter. The seventh parameter, OrderTitle, ' + 
				'is missing or blank. OrderTitle must be a non-null string with at ' +
				'least one non-whitespace character.';
			SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
			RAISERROR(
				'%s - %s', 
				15, 
				107,
				@textTimestamp,
				@textMessage
			);
			END;

--	Titles don't have to be for a unique agency
--		IF EXISTS(
--			SELECT
--				*
--			FROM
--				wb.wb_schedules
--			WHERE
--				@idMediaWeek = media_week_id
--				AND
--				@titleOrder = order_title
--				AND
--				@idWBAgency <> wb_agency_id
--			)
--			BEGIN
--			SET @textMessage = 
--				'Invalid parameter. The seventh parameter, OrderTitle, ' + 
--				'which has a value of ''' + @titleOrder + ''',' +
--				'is already associated with an agency other than ' + 
--				@AgencyName + ' in this week''s schedule.';
--			SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
--			RAISERROR(
--				'%s - %s', 
--				15, 
--				107,
--				@textTimestamp,
--				@textMessage
--			);
--			END;

		--Validate eighth parameter, Status...
		SET @codeStatus = LTRIM(RTRIM(ISNULL(@Status, '')));
		IF LEN(@codeStatus) = 0
			BEGIN
			SET @textMessage = 
				'Invalid parameter. The eighth parameter, Status, ' + 
				'is missing or blank. Status must be a non-null string with at ' +
				'least one non-whitespace character.';
			SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
			RAISERROR(
				'%s - %s', 
				15, 
				108,
				@textTimestamp,
				@textMessage
			);
			END;

		--Validate ninth parameter, NetRate...
		IF ISNUMERIC(@NetRate) <> 1
			BEGIN
			SET @textMessage = 
				'Invalid parameter. The ninth parameter, NetRate, ' + 
				'which has a value of ''' + ISNULL(@NetRate, '<NULL>') + 
				''', is not an dollar value.';
			SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
			RAISERROR(
				'%s - %s', 
				15, 
				109,
				@textTimestamp,
				@textMessage
			);
			END;
		SET @rateNet = CAST(@NetRate AS MONEY);
		IF @rateNet < 0
			BEGIN
			SET @textMessage = 
				'Invalid parameter. The ninth parameter, NetRate, ' + 
				'which has a value of ''$' + CONVERT(VARCHAR, @rateNet, 1) + 
				''', cannot be less than $0.00.';
			SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
			RAISERROR(
				'%s - %s', 
				15, 
				109,
				@textTimestamp,
				@textMessage,
				@timeStart,
				@timeEnd
			);
			END;

		--Validate tenth parameter, AgencyPercentage...
		IF ISNUMERIC(@AgencyPercentage) <> 1
			BEGIN
			SET @textMessage = 
				'Invalid parameter. The tenth parameter, AgencyPercentage, ' + 
				'which has a value of ''' + ISNULL(@AgencyPercentage, '<NULL>') + 
				''', is not a valid value.';
			SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
			RAISERROR(
				'%s - %s', 
				15, 
				110,
				@textTimestamp,
				@textMessage
			);
			END;
		IF (0.0 > CAST(@AgencyPercentage AS FLOAT)) OR (CAST(@AgencyPercentage AS FLOAT) > 1.0)
			BEGIN
			SET @textMessage = 
				'Invalid parameter. The tenth parameter, AgencyPercentage, ' + 
				'which has a value of ''' + 
				CONVERT(VARCHAR, CAST(@AgencyPercentage AS MONEY) * 100.0, 1) + 
				'%'', must have a value between 0% and 100%.';
			SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
			RAISERROR(
				'%s - %s', 
				15, 
				110,
				@textTimestamp,
				@textMessage,
				@timeStart,
				@timeEnd
			);
			END;
		SET @percentageAgency = CAST(@AgencyPercentage AS DECIMAL(10,9));

		--Validate eleventh parameter, GrossRate...
		IF ISNUMERIC(@GrossRate) <> 1
			BEGIN
			SET @textMessage = 
				'Invalid parameter. The eleventh parameter, GrossRate, ' + 
				'which has a value of ''' + ISNULL(@GrossRate, '<NULL>') + 
				''', is not an dollar value.';
			SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
			RAISERROR(
				'%s - %s', 
				15, 
				111,
				@textTimestamp,
				@textMessage
			);
			END;
		SET @rateGross = CAST(@GrossRate AS MONEY);
		IF @rateGross < 0
			BEGIN
			SET @textMessage = 
				'Invalid parameter. The eleventh parameter, GrossRate, ' + 
				'which has a value of ''$' + CONVERT(VARCHAR, @rateGross, 1) + 
				''', cannot be less than $0.00.';
			SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
			RAISERROR(
				'%s - %s', 
				15, 
				111,
				@textTimestamp,
				@textMessage,
				@timeStart,
				@timeEnd
			);
			END;
			
		IF abs((@rateNet / (1.0 - @percentageAgency)) - @rateGross) > 0.0075
			BEGIN
			SET @textMessage = 
				'Invalid parameter. The eleventh parameter, GrossRate, ' + 
				'which has a value of $' + CONVERT(VARCHAR, @rateGross, 1) + 
				'. GrossRate should be $' + 
				CONVERT(
					VARCHAR, 
					CAST(@rateNet / (1.0 - @percentageAgency) AS MONEY), 
					1) + 
				'.';
			SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
			RAISERROR(
				'%s - %s', 
				15, 
				111,
				@textTimestamp,
				@textMessage,
				@timeStart,
				@timeEnd
			);
			END;

		--Validate twelfth parameter, TapeID...
		SET @codeTape = LTRIM(RTRIM(@TapeID));
		IF (@codeTape IS NOT NULL) AND (LEN(@codeTape) = 0)
			BEGIN
			SET @codeTape = NULL;
			END;

		--IF EXISTS(
		--	SELECT
		--		*
		--	FROM
		--		wb.wb_schedules
		--	WHERE
		--		@idMediaWeek = media_week_id
		--		AND
		--		@titleOrder = order_title
		--		AND
		--		ISNULL(@codeTape, '') <> ISNULL(tape_id, '')
		--	)
		--	BEGIN
		--	SET @textMessage = 
		--		'Invalid parameter. The order in this schedule line, ' +
		--		@titleOrder + ', is already associated with different ' + 
		--		'TAPEID than the one in the twelfth parameter, TapeID, ''' + 
		--		@codeTape + '''.';
		--	SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		--	RAISERROR(
		--		'%s - %s', 
		--		15, 
		--		107,
		--		@textTimestamp,
		--		@textMessage
		--	);
		--	END;

		--Validate thirteenth parameter, PhoneNumber...
		SET @codePhoneNumber = LTRIM(RTRIM(@PhoneNumber));
		IF (@codePhoneNumber IS NOT NULL) AND (LEN(@codePhoneNumber) = 0)
			BEGIN
			SET @codePhoneNumber = NULL;
			END;

		--IF EXISTS(
		--	SELECT
		--		*
		--	FROM
		--		wb.wb_schedules
		--	WHERE
		--		@idMediaWeek = media_week_id
		--		AND
		--		@titleOrder = order_title
		--		AND
		--		ISNULL(@codePhoneNumber, '') <> ISNULL(phone_number, '')
		--	)
		--	BEGIN
		--	SET @textMessage = 
		--		'Invalid parameter. The order in this schedule line, ' +
		--		@titleOrder + ', is already associated with different ' + 
		--		'phone number than the one in the thirteenth parameter, PhoneNumber, ''' + 
		--		@codePhoneNumber + '''.';
		--	SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		--	RAISERROR(
		--		'%s - %s', 
		--		15, 
		--		113,
		--		@textTimestamp,
		--		@textMessage
		--	);
		--	END;

		--Validate fourteenth parameter, SalesPerson...
		SET @nameSalesPerson = LTRIM(RTRIM(@SalesPerson));
		IF (@nameSalesPerson IS NOT NULL) AND (LEN(@nameSalesPerson) = 0)
			BEGIN
			SET @nameSalesPerson = NULL;
			END;

		IF EXISTS(
			SELECT
				*
			FROM
				wb.wb_schedules
			WHERE
				@idMediaWeek = media_week_id
				AND
				@titleOrder = order_title
				AND
				@codeRegion = region_code
				AND
				ISNULL(@nameSalesPerson, '') <> ISNULL(sales_person, '')
			)
			BEGIN
			SET @textMessage = 
				'Invalid parameter. The order in this schedule line, ' +
				@titleOrder + ', is already associated with different ' + 
				'sales person than the one in the fourteenth parameter, SalesPerson, ''' + 
				@nameSalesPerson + '''.';
			SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
			RAISERROR(
				'%s - %s', 
				15, 
				114,
				@textTimestamp,
				@textMessage
			);
			END;

		--Validate fifteenth parameter, RateCardRate...
		IF ISNUMERIC(@RateCardRate) <> 1
			BEGIN
			SET @textMessage = 
				'Invalid parameter. The fifteenth parameter, RateCardRate, ' + 
				'which has a value of ''' + ISNULL(@RateCardRate, '<NULL>') + 
				''', is not an dollar value.';
			SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
			RAISERROR(
				'%s - %s', 
				15, 
				115,
				@textTimestamp,
				@textMessage
			);
			END;
		SET @rateRateCard = CAST(@RateCardRate AS MONEY);
		--IF @rateRateCard <= 0
		--	BEGIN
		--	SET @textMessage = 
		--		'Invalid parameter. The fifteenth parameter, RateCardRate, ' + 
		--		'which has a value of ''$' + CONVERT(VARCHAR, @rateRateCard, 1) + 
		--		''', must have a value greater than $0.00.';
		--	SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		--	RAISERROR(
		--		'%s - %s', 
		--		15, 
		--		115,
		--		@textTimestamp,
		--		@textMessage,
		--		@timeStart,
		--		@timeEnd
		--	);
		--	END;

		BEGIN TRANSACTION;
		insert into
			wb.wb_schedules(
				region_code, 
				media_week_id, 
				start_time, 
				end_time, 
				day_id, 
				wb_agency_id, 
				order_title, 
				status, 
				net_rate, 
				agency_percent, 
				gross_rate, 
				tape_id, 
				phone_number, 
				sales_person, 
				rate_card_rate
			)
		values(
			@codeRegion,
			@idMediaWeek,
			@timeStart,
			@timeEnd,
			@idDay,
			@idWBAgency,
			@titleOrder,
			@codeStatus,
			@rateNet,
			@percentageAgency,
			@rateGross,
			@codeTape,
			@codePhoneNumber,
			@nameSalesPerson,
			@rateRatecard
		);

		COMMIT TRANSACTION;   
	END TRY
	BEGIN CATCH
        -- Re-raise the original error.
        EXEC wb.usp_RethrowError;
	END CATCH;
END;