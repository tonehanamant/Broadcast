-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/31/2015
-- Description:	Algorithm used to populate rotational_bias_coefficients. It clears out any existing data for the media_month_id first.
-- Runtime:		
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARS_CalculateRotationalBiasCoefficients]
	@rule_code TINYINT,
	@base_media_month_id INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @textTimestamp VARCHAR(63)

	IF OBJECT_ID('tempdb..#normalized_data') IS NOT NULL DROP TABLE #normalized_data;
	CREATE TABLE #normalized_data (media_month_id INT NOT NULL, week_number TINYINT NOT NULL, network_id INT NOT NULL, hour_of_week TINYINT NOT NULL, business_id INT NOT NULL, subscribers BIGINT NOT NULL);
	--ALTER TABLE  #normalized_data ADD PRIMARY KEY CLUSTERED (media_month_id,week_number,network_id,hour_of_week,business_id);

	IF OBJECT_ID('tempdb..#forecast_media_months_year_1') IS NOT NULL DROP TABLE #forecast_media_months_year_1;
	CREATE TABLE #forecast_media_months_year_1 (media_month_id INT NOT NULL);
	ALTER TABLE  #forecast_media_months_year_1 ADD PRIMARY KEY CLUSTERED (media_month_id);

	IF OBJECT_ID('tempdb..#forecast_media_months_year_2') IS NOT NULL DROP TABLE #forecast_media_months_year_2;
	CREATE TABLE #forecast_media_months_year_2 (media_month_id INT NOT NULL);
	ALTER TABLE  #forecast_media_months_year_2 ADD PRIMARY KEY CLUSTERED (media_month_id);

	DECLARE @start_forecast_month DATETIME;
	DECLARE @end_forecast_month DATETIME;

	SELECT @start_forecast_month = mm.start_date FROM dbo.media_months mm (NOLOCK) WHERE mm.id=@base_media_month_id;
	SELECT @end_forecast_month = mm.start_date FROM dbo.media_months mm (NOLOCK) WHERE mm.id=dbo.udf_CalculateFutureMediaMonthId(@base_media_month_id, 11)
	INSERT INTO #forecast_media_months_year_1
		SELECT
			mm.id
		FROM
			dbo.media_months mm (NOLOCK)
		WHERE
			(mm.start_date <= @end_forecast_month AND mm.end_date >= @start_forecast_month)
		ORDER BY
			mm.start_date
		
	SELECT @start_forecast_month = mm.start_date FROM dbo.media_months mm (NOLOCK) WHERE mm.id=dbo.udf_CalculateFutureMediaMonthId(@base_media_month_id, 12)
	SELECT @end_forecast_month = mm.start_date FROM dbo.media_months mm (NOLOCK) WHERE mm.id=dbo.udf_CalculateFutureMediaMonthId(@base_media_month_id, 24)
	INSERT INTO #forecast_media_months_year_2
		SELECT
			mm.id
		FROM
			dbo.media_months mm (NOLOCK)
		WHERE
			(mm.start_date <= @end_forecast_month AND mm.end_date >= @start_forecast_month)
		ORDER BY
			mm.start_date	

	DECLARE @forecast_media_month_id AS INT
	DECLARE @forecast_media_month_num_weeks AS TINYINT
	DECLARE @input_media_month_ids UniqueIdTable;
	DECLARE ForecastMonthCursor CURSOR FAST_FORWARD FOR
		SELECT fmm.media_month_id,COUNT(1) FROM dbo.media_weeks mw (NOLOCK) JOIN #forecast_media_months_year_1 fmm ON fmm.media_month_id=mw.media_month_id GROUP BY fmm.media_month_id
	OPEN ForecastMonthCursor
	FETCH NEXT FROM ForecastMonthCursor INTO @forecast_media_month_id,@forecast_media_month_num_weeks
	WHILE @@FETCH_STATUS = 0
		BEGIN
			DELETE FROM @input_media_month_ids;
		
			IF @rule_code = 0
			BEGIN
				-- static
				INSERT INTO @input_media_month_ids SELECT dbo.udf_CalculateFutureMediaMonthId(@forecast_media_month_id, -1)
				INSERT INTO @input_media_month_ids SELECT dbo.udf_CalculateFutureMediaMonthId(@forecast_media_month_id, -2)
				INSERT INTO @input_media_month_ids SELECT dbo.udf_CalculateFutureMediaMonthId(@forecast_media_month_id, -3)
			END
			ELSE IF @rule_code = 1
			BEGIN
				-- seasonal
				INSERT INTO @input_media_month_ids SELECT dbo.udf_CalculateFutureMediaMonthId(@forecast_media_month_id, -11)
				INSERT INTO @input_media_month_ids SELECT dbo.udf_CalculateFutureMediaMonthId(@forecast_media_month_id, -12)
				INSERT INTO @input_media_month_ids SELECT dbo.udf_CalculateFutureMediaMonthId(@forecast_media_month_id, -13)
			END
			ELSE IF @rule_code = 2
			BEGIN
				-- hybrid
				INSERT INTO @input_media_month_ids SELECT dbo.udf_CalculateFutureMediaMonthId(@forecast_media_month_id, -1)
				INSERT INTO @input_media_month_ids SELECT dbo.udf_CalculateFutureMediaMonthId(@forecast_media_month_id, -2)
				INSERT INTO @input_media_month_ids SELECT dbo.udf_CalculateFutureMediaMonthId(@forecast_media_month_id, -3)
				INSERT INTO @input_media_month_ids SELECT dbo.udf_CalculateFutureMediaMonthId(@forecast_media_month_id, -11)
				INSERT INTO @input_media_month_ids SELECT dbo.udf_CalculateFutureMediaMonthId(@forecast_media_month_id, -12)
				INSERT INTO @input_media_month_ids SELECT dbo.udf_CalculateFutureMediaMonthId(@forecast_media_month_id, -13)
			END
		
			--DEBUG
			--SELECT @forecast_media_month_id,* FROM @input_media_month_ids
				
			DECLARE @num_input_months INT;
			SELECT @num_input_months = COUNT(1) FROM @input_media_month_ids;
				
			-- perform validation that the necessary data exists
			DECLARE @num_input_months_we_have_data_for INT;
			SELECT
				@num_input_months_we_have_data_for = COUNT(DISTINCT media_month_id)
			FROM
				[mart].[rotational_bias_inputs] rbi
				JOIN @input_media_month_ids immi ON immi.id=rbi.media_month_id;
		
			IF @num_input_months_we_have_data_for <> @num_input_months
			BEGIN
				SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
				RAISERROR('%s - The number of media months in [mart].[rotational_bias_inputs] needed to run the rule %d algorithm for forecast month %d must be %d total and we only have %d available.', 0, 1, @textTimestamp, @rule_code, @forecast_media_month_id, @num_input_months, @num_input_months_we_have_data_for) WITH NOWAIT;
				RETURN;
			END		
		
			DECLARE @num_weeks_in_input_month TINYINT;
			DECLARE @input_media_month_id INT;
			DECLARE RoleMonthsCursor CURSOR FAST_FORWARD FOR
				SELECT mm.id,COUNT(1) FROM dbo.media_weeks mw (NOLOCK) JOIN @input_media_month_ids mm ON mm.id=mw.media_month_id GROUP BY mm.id
			OPEN RoleMonthsCursor
			FETCH NEXT FROM RoleMonthsCursor INTO @input_media_month_id,@num_weeks_in_input_month
			WHILE @@FETCH_STATUS = 0
				BEGIN				
					IF @num_weeks_in_input_month = 4 AND @forecast_media_month_num_weeks = 5
						BEGIN
							-- normalize (week 1)
							INSERT INTO #normalized_data
								SELECT
									@forecast_media_month_id,
									rbi.week_number,
									rbi.network_id,
									rbi.hour_of_week,
									rbi.business_id,
									rbi.subscribers
								FROM
									[mart].[rotational_bias_inputs] rbi (NOLOCK)
								WHERE
									rbi.media_month_id=@input_media_month_id
									AND rbi.week_number=1
							-- normalize (week 2 [mean of weeks 2 and 3])
							INSERT INTO #normalized_data
								SELECT 
									@forecast_media_month_id,
									2,
									rbi.network_id,
									rbi.hour_of_week,
									rbi.business_id,
									AVG(rbi.subscribers)
								FROM 
									[mart].[rotational_bias_inputs] rbi (NOLOCK)
								WHERE 
									rbi.media_month_id=@input_media_month_id
									AND rbi.week_number IN (2,3)
								GROUP BY
									rbi.media_month_id,
									rbi.network_id,
									rbi.hour_of_week,
									rbi.business_id
							-- normalize (week 3 [mean of weeks 3 and 4])
							INSERT INTO #normalized_data
								SELECT 
									@forecast_media_month_id,
									3,
									rbi.network_id,
									rbi.hour_of_week,
									rbi.business_id,
									AVG(rbi.subscribers)
								FROM 
									[mart].[rotational_bias_inputs] rbi (NOLOCK)
								WHERE 
									rbi.media_month_id=@input_media_month_id
									AND rbi.week_number IN (3,4)
								GROUP BY
									rbi.media_month_id,
									rbi.network_id,
									rbi.hour_of_week,
									rbi.business_id
							-- normalize (week 4 [equal to week 5])
							INSERT INTO #normalized_data
								SELECT 
									@forecast_media_month_id,
									4,
									rbi.network_id,
									rbi.hour_of_week,
									rbi.business_id,
									rbi.subscribers
								FROM 
									[mart].[rotational_bias_inputs] rbi (NOLOCK)
								WHERE 
									rbi.media_month_id=@input_media_month_id
									AND rbi.week_number=5
						END
					ELSE
					IF @num_weeks_in_input_month = 5 AND @forecast_media_month_num_weeks = 4
						BEGIN
							-- normalize (week 1)
							INSERT INTO #normalized_data
								SELECT 
									@forecast_media_month_id,
									rbi.week_number,
									rbi.network_id,
									rbi.hour_of_week,
									rbi.business_id,
									rbi.subscribers
								FROM 
									[mart].[rotational_bias_inputs] rbi (NOLOCK)
								WHERE 
									rbi.media_month_id=@input_media_month_id
									AND rbi.week_number=1
							-- normalize (week 2)
							INSERT INTO #normalized_data
								SELECT 
									@forecast_media_month_id,
									rbi.week_number,
									rbi.network_id,
									rbi.hour_of_week,
									rbi.business_id,
									rbi.subscribers
								FROM 
									[mart].[rotational_bias_inputs] rbi (NOLOCK)
								WHERE 
									rbi.media_month_id=@input_media_month_id
									AND rbi.week_number=2
							-- normalize (week 3 [mean of weeks 2 and 3])
							INSERT INTO #normalized_data
								SELECT 
									@forecast_media_month_id,
									3,
									rbi.network_id,
									rbi.hour_of_week,
									rbi.business_id,
									AVG(rbi.subscribers)
								FROM 
									[mart].[rotational_bias_inputs] rbi (NOLOCK)
								WHERE 
									rbi.media_month_id=@input_media_month_id
									AND rbi.week_number IN (2,3)
								GROUP BY
									rbi.media_month_id,
									rbi.network_id,
									rbi.hour_of_week,
									rbi.business_id
							-- normalize (week 4 [equal to week 3])
							INSERT INTO #normalized_data
								SELECT 
									@forecast_media_month_id,
									4,
									rbi.network_id,
									rbi.hour_of_week,
									rbi.business_id,
									rbi.subscribers
								FROM 
									[mart].[rotational_bias_inputs] rbi (NOLOCK)
								WHERE 
									rbi.media_month_id=@input_media_month_id
									AND rbi.week_number=3
							-- normalize (week 5 [equal to week 4])
							INSERT INTO #normalized_data
								SELECT 
									@forecast_media_month_id,
									5,
									rbi.network_id,
									rbi.hour_of_week,
									rbi.business_id,
									rbi.subscribers
								FROM 
									[mart].[rotational_bias_inputs] rbi (NOLOCK)
								WHERE 
									rbi.media_month_id=@input_media_month_id
									AND rbi.week_number=4
						END
					ELSE
						BEGIN
							-- dump as is
							INSERT INTO #normalized_data
								SELECT 
									@forecast_media_month_id,
									rbi.week_number,
									rbi.network_id,
									rbi.hour_of_week,
									rbi.business_id,
									rbi.subscribers
								FROM 
									[mart].[rotational_bias_inputs] rbi (NOLOCK)
								WHERE 
									rbi.media_month_id=@input_media_month_id
						END
					
					FETCH NEXT FROM RoleMonthsCursor INTO @input_media_month_id,@num_weeks_in_input_month
				END
			CLOSE RoleMonthsCursor
			DEALLOCATE RoleMonthsCursor	

			FETCH NEXT FROM ForecastMonthCursor INTO @forecast_media_month_id,@forecast_media_month_num_weeks
		END
	CLOSE ForecastMonthCursor
	DEALLOCATE ForecastMonthCursor	

	DELETE FROM rotational_bias_coefficients WHERE rule_code=@rule_code AND base_media_month_id=@base_media_month_id;

	INSERT INTO rotational_bias_coefficients (rule_code, base_media_month_id, forecast_media_month_id, week_number, network_id, hour_of_week, business_id, subscribers)
		SELECT
			@rule_code,
			@base_media_month_id,
			nd.media_month_id,
			nd.week_number,
			nd.network_id,
			nd.hour_of_week,
			nd.business_id,
			SUM(nd.subscribers)
		FROM
			#normalized_data nd
		GROUP BY
			nd.media_month_id,
			nd.week_number,
			nd.network_id,
			nd.hour_of_week,
			nd.business_id;

	-- copy year 2 from year 1
	INSERT INTO rotational_bias_coefficients (rule_code, base_media_month_id, forecast_media_month_id, week_number, network_id, hour_of_week, business_id, subscribers)
		SELECT
			rbc.rule_code,
			rbc.base_media_month_id,
			fmmy2.media_month_id,
			rbc.week_number,
			rbc.network_id,
			rbc.hour_of_week,
			rbc.business_id,
			rbc.subscribers
		FROM
			rotational_bias_coefficients rbc (NOLOCK)
			JOIN #forecast_media_months_year_2 fmmy2 ON fmmy2.media_month_id=dbo.udf_CalculateFutureMediaMonthId(rbc.forecast_media_month_id, 12)
		WHERE
			rbc.base_media_month_id=@base_media_month_id
			AND rbc.rule_code=@rule_code;
		
	DROP TABLE #normalized_data;
	DROP TABLE #forecast_media_months_year_1;
	DROP TABLE #forecast_media_months_year_2;
END