CREATE procedure [dbo].[usp_ARSLoader_ForecastingUniverses]
	@lMedia_month_id INT,
	@RatingCategoryId INT
AS
BEGIN
	DECLARE 
		--@lMedia_month_id INT,
		@szDateForecast AS VARCHAR(15),
		@lForecast_media_month_id INT,
		@lMonthDiff INT,
		@lBaseDate DATETIME;
	
	DECLARE 
		@lNetworksToExclude TABLE(
			nielsen_network_id INT
		);
	
	BEGIN TRY
		BEGIN TRANSACTION;
		SET @lMonthDiff =1
	
		SELECT 
			@lMedia_month_id = mm.id, 
			@lBaseDate = dateadd(d, 15, mm.start_date) 
		FROM 
			media_months mm (NOLOCK)
		WHERE 
			mm.id = @lMedia_month_id;
	
		INSERT INTO @lNetworksToExclude
			SELECT DISTINCT 
				nielsen_network_id
			FROM 
				universes u (NOLOCK)
			WHERE 
				rating_category_id = @RatingCategoryId
				AND base_media_month_id = @lMedia_month_id
				AND forecast_media_month_id = @lMedia_month_id
				AND nielsen_network_id NOT IN (
					SELECT DISTINCT 
						u2.nielsen_network_id
					FROM 
						universes u2 (NOLOCK)
					INNER JOIN media_months mm ON u2.base_media_month_id = mm.id
					WHERE 
						u2.rating_category_id = @RatingCategoryId
						AND mm.year * 100 + mm.month = (
							SELECT 
								[year] * 100 + [month] 
							FROM 
								media_months (NOLOCK)
							WHERE 
								dateadd(month, -23, @lBaseDate) BETWEEN start_date AND end_date
						)
						AND u2.base_media_month_id = u2.forecast_media_month_id
				);
	
		DELETE FROM 
			universes 
		WHERE
			rating_category_id = @RatingCategoryId
			AND base_media_month_id = @lMedia_month_id 
			AND base_media_month_id <> forecast_media_month_id;
	
		WHILE (@lMonthDiff<=24)
		BEGIN
	
			SELECT 
				@lForecast_media_month_id = id 
			FROM 
				media_months (NOLOCK)
			WHERE 
				(
					SELECT 
						dateadd(M, @lMonthDiff, dateadd(d, 15, start_date)) 
					FROM 
						media_months (NOLOCK)
					WHERE 
						id=@lMedia_month_id
				) BETWEEN start_date AND end_date;
			IF @lForecast_media_month_id IS NULL
			BEGIN
				SELECT 
					@szDateForecast = convert(varchar, dateadd(M, @lMonthDiff, dateadd(d, 15, start_date)), 111)
				FROM 
					media_months 
				WHERE 
					id=@lMedia_month_id;
					
				raiserror(
					'A media month for the requested forecast date, %s, IS NOT IN the media_months table.', 
					15, 
					1, 
					@szDateForecast);
			END
			
	
			SELECT 
				@lMonthDiff= datediff (	month,
								cast (cast(bmm.month AS VARCHAR(5)) + '/1/' + cast(bmm.year AS VARCHAR(5)) AS DATETIME),
								cast (cast(fmm.month AS VARCHAR(5)) + '/1/' + cast(fmm.year AS VARCHAR(5)) AS DATETIME)) 
			FROM 
				media_months fmm (NOLOCK), 
				media_months bmm (NOLOCK)
			WHERE 
				fmm.id= @lForecast_media_month_id 
				AND bmm.id=@lMedia_month_id;
	
			INSERT INTO universes(
				rating_category_id,
				base_media_month_id,
				forecast_media_month_id,
				nielsen_network_id,
				audience_id,
				universe
			)
			SELECT
				@RatingCategoryId [rating_category_id],
				@lMedia_month_id [base_media_month_id],
				@lForecast_media_month_id [forecast_media_month_id],
				nn.id,
				a_c.id [audience_id],
				SUM(base.universe) * power (power( SUM(year1.universe) / SUM(year2.universe), (1.0/12.0)), @lMonthDiff) [forecast]
				--power (power( SUM(year1.universe) / SUM(year2.universe) , (1.0/12.0)), @lMonthDiff)
			FROM (
				SELECT 
					nielsen_network_id, audience_id, universe
				FROM 
					universes u (NOLOCK)
				WHERE 
					rating_category_id = @RatingCategoryId
					AND base_media_month_id = @lMedia_month_id
					AND base_media_month_id = forecast_media_month_id
			) base
			INNER JOIN (
				SELECT 
					nielsen_network_id, audience_id, SUM(universe) [universe]
				FROM 
					universes u (NOLOCK)
					INNER JOIN media_months mm (NOLOCK) ON mm.id=u.base_media_month_id
				WHERE 
					mm.year * 100 + mm.month BETWEEN
					(SELECT [year] * 100 + [month] FROM media_months (NOLOCK) WHERE dateadd(month, -11, @lBaseDate) BETWEEN start_date AND end_date)
						AND
					(SELECT [year] * 100 + [month] FROM media_months (NOLOCK) WHERE id= @lMedia_month_id)
					AND u.base_media_month_id = u.forecast_media_month_id
					AND u.rating_category_id = @RatingCategoryId
				GROUP BY 
					nielsen_network_id, audience_id
				) year1 ON year1.nielsen_network_id=base.nielsen_network_id AND year1.audience_id=base.audience_id
			INNER JOIN (
				SELECT 
					nielsen_network_id, audience_id, SUM(universe) [universe]
				FROM 
					universes u (NOLOCK)
					INNER JOIN media_months mm (NOLOCK) ON (u.base_media_month_id = mm.id)
				WHERE
					mm.year * 100 + mm.month BETWEEN
					(SELECT [year] * 100 + [month] FROM media_months (NOLOCK) WHERE dateadd(month, -23, @lBaseDate) BETWEEN start_date AND end_date)
						AND
					(SELECT [year] * 100 + [month] FROM media_months (NOLOCK) WHERE dateadd(month, -12, @lBaseDate) BETWEEN start_date AND end_date)
					AND u.base_media_month_id = u.forecast_media_month_id
					AND u.rating_category_id = @RatingCategoryId
				GROUP BY 
					nielsen_network_id, audience_id
				) year2 ON year1.nielsen_network_id=year2.nielsen_network_id AND year1.audience_id=year2.audience_id
			JOIN audiences a_c (NOLOCK) ON a_c.id = base.audience_id
			JOIN nielsen_networks nn (NOLOCK) ON base.nielsen_network_id = nn.id
			WHERE nn.id NOT IN (
				SELECT nielsen_network_id FROM @lNetworksToExclude
			)
			GROUP BY nn.id, a_c.id
			ORDER BY nn.id, a_c.id
	
			SET @lMonthDiff = @lMonthDiff + 1
			
		END
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
