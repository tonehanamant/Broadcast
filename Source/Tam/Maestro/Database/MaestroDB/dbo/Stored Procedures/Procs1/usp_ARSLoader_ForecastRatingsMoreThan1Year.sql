CREATE procedure [dbo].[usp_ARSLoader_ForecastRatingsMoreThan1Year]
	@lMedia_month_id INT,
	@RatingCategoryId INT
AS
BEGIN
	DECLARE
		@lBase_media_month_id AS INT,
		@lRating_media_month_id AS INT,
		@lForecast_media_month_id AS INT,
		@szBase_media_month AS VARCHAR(7),
		@szRating_media_month AS VARCHAR(7),
		@szForecast_media_month varchar(7),
		@lMonthDiff INT;
	
	BEGIN TRY
		BEGIN TRANSACTION;
		
		DECLARE @dayparts TABLE(
			daypart_id INT
		);
		INSERT INTO @dayparts(daypart_id)
			SELECT DISTINCT dpm.daypart_id FROM dbo.daypart_maps dpm (NOLOCK) WHERE dpm.map_set IN ('Ratings', 'FM_nDyptNum');
	
		DECLARE @nielsen_networks TABLE(
			nielsen_network_id INT
		);
		INSERT INTO @nielsen_networks(nielsen_network_id)
			SELECT nn.id FROM  dbo.nielsen_networks nn (NOLOCK);
	
		SELECT @lBase_media_month_id = mm.id FROM dbo.media_months mm (NOLOCK) WHERE mm.id=@lMedia_month_id;
		 
		SET @lMonthDiff = 1;
		 
		WHILE (@lMonthDiff <= 12)
		BEGIN
			-- Determine month to get delivery forecasts FROM
			SELECT 
				@lRating_media_month_id = mm.id 
			FROM 
				dbo.media_months mm (NOLOCK)
			WHERE 
				(
					SELECT 
						dateadd(M, @lMonthDiff, dateadd(d, 15, start_date)) 
					FROM 
						dbo.media_months (NOLOCK) 
					WHERE 
						id = @lBase_media_month_id
				) BETWEEN mm.start_date AND mm.end_date;
	
			-- Determine month to forecast delivery for
			SELECT 
				@lForecast_media_month_id = mm.id 
			FROM 
				dbo.media_months mm (NOLOCK)
			WHERE 
				(
					SELECT 
						dateadd(M, @lMonthDiff + 12, dateadd(d, 15, start_date)) 
					FROM 
						dbo.media_months (NOLOCK)
					WHERE 
						id = @lBase_media_month_id
				) BETWEEN mm.start_date AND mm.end_date;
	
			SELECT	@szRating_media_month = media_month		FROM dbo.media_months (NOLOCK) WHERE id = @lRating_media_month_id;
			SELECT	@szForecast_media_month = media_month	FROM dbo.media_months (NOLOCK) WHERE id = @lForecast_media_month_id;
			SELECT	@szBase_media_month = media_month		FROM dbo.media_months (NOLOCK) WHERE id = @lBase_media_month_id;
			PRINT 'Ratings for ' + @szForecast_media_month + ' based ON ' + @szRating_media_month + ':' + @szBase_media_month;
	
			INSERT INTO ratings
			(
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
				r.rating_category_id,
				@lBase_media_month_id [base_media_month_id],
				@lForecast_media_month_id [forecast_media_month_id],
				r.nielsen_network_id, 
				r.audience_id, 
				r.daypart_id,
				(r.audience_usage/u.universe) * u2.universe audience_usage,
				(r.tv_usage/u.universe) * u2.universe tv_usage				
			FROM	
				dbo.ratings r (NOLOCK)
				INNER JOIN dbo.universes u (NOLOCK) ON  
					u.rating_category_id = r.rating_category_id
					AND u.base_media_month_id = r.base_media_month_id 
					AND u.base_media_month_id = u.forecast_media_month_id
					AND u.nielsen_network_id = r.nielsen_network_id 
					AND u.audience_id = r.audience_id					 
				INNER JOIN dbo.universes u2 (NOLOCK) ON  
					u2.rating_category_id = r.rating_category_id
					AND u2.base_media_month_id = r.base_media_month_id 
					AND u2.forecast_media_month_id = @lForecast_media_month_id
					AND u2.nielsen_network_id = r.nielsen_network_id 
					AND u2.audience_id = r.audience_id
			WHERE 
				r.rating_category_id = @RatingCategoryId
				AND r.base_media_month_id = @lBase_media_month_id
				AND r.forecast_media_month_id = @lRating_media_month_id
				AND r.nielsen_network_id IN (
					SELECT nielsen_network_id FROM @nielsen_networks
				)
				AND r.daypart_id IN (
					SELECT daypart_id FROM @dayparts
				)
			ORDER BY 
				r.nielsen_network_id, 
				r.audience_id, 
				r.daypart_id,
				r.base_media_month_id,
				r.forecast_media_month_id;
	
			SET @lMonthDiff = @lMonthDiff + 1;
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
