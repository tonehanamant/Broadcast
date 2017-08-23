--Removed rotational_bias factor - 
--    Database will contain forecasts based ON base month AND seasonality.
--    Database will also contain adjustment factors. 
--    ARS will apply adjustment factors to forecasts.
CREATE procedure [dbo].[usp_ARSLoader_ForecastingRatings]
	@Media_month varchar(5),
	@Rating_category varchar(15)
AS
BEGIN
	DECLARE
		@lMedia_month_id INT,
		@Rating_category_id INT,
		@generalCategoryID AS INT,
		@particularCategoryID AS INT,
		@nonseasonalCategoryID AS INT,
		@generalNetworkID AS INT,
		@month AS DATETIME,
		@firstForecastMonth AS DATETIME,
		@lastForecastMonth AS DATETIME,
		@baseMediaMonthID AS INT,
		@baseM12MediaMonthID AS INT,
		@baseM24MediaMonthID AS INT;
	
	DECLARE @nielsen_networks TABLE(
		nielsen_network_id INT
	);
	
	BEGIN TRY
		BEGIN TRANSACTION;
		INSERT INTO @nielsen_networks(nielsen_network_id)
			SELECT
				nn.id
			FROM 
				nielsen_networks nn (NOLOCK);
		--	WHERE
		--		nn.code IN('adsm', 'nan', 'nick', 'toon');
	
		DECLARE @dayparts TABLE(
			daypart_id INT
		);
	
		INSERT INTO @dayparts(daypart_id)
			SELECT DISTINCT
				daypart_id
			FROM 
				daypart_maps dpm (NOLOCK)
			WHERE
				dpm.map_set IN ('Ratings', 'FM_nDyptNum');
	
	
		-- Initialize local variables
		SELECT
			@lMedia_month_id = id
		FROM
			media_months (NOLOCK)
		WHERE
			media_month = @Media_month;
	
		SELECT
			@Rating_category_id = id
		FROM
			rating_categories (NOLOCK)
		WHERE
			code = @Rating_category;
	
		SELECT
			@generalCategoryID = id
		FROM
			network_rating_categories nrc (NOLOCK)
		WHERE
			name = 'General';
	
		SELECT
			@particularCategoryID = id
		FROM
			network_rating_categories nrc (NOLOCK)
		WHERE
			name = 'Particular';
	
		SELECT
			@nonseasonalCategoryID = id
		FROM
			network_rating_categories nrc (NOLOCK)
		WHERE
			name = 'Non-seasonal';
	
		SELECT
			@generalNetworkID = nn.id
		FROM
			nielsen_networks nn (NOLOCK)
		WHERE
			nn.code = 'M_GEN';
	
		SET @baseMediaMonthID = @lMedia_month_id;
		SELECT
			@month = dateadd(day,14,dbo.Period2FirstOfMonth(media_month))
		FROM
			media_months (NOLOCK)
		WHERE
			id = @baseMediaMonthID;
	
		SELECT
			@baseM12MediaMonthID = id
		FROM
			media_months (NOLOCK)
		WHERE
			dateadd(year, -1, @month) BETWEEN start_date AND end_date;
	
		SELECT
			@baseM24MediaMonthID = id
		FROM
			media_months (NOLOCK)
		WHERE
			dateadd(year, -2, @month) BETWEEN start_date AND end_date;
	
		SELECT
			@firstForecastMonth = dateadd( month, 1, @month ),
			@lastForecastMonth = dateadd( month, 12, @month );
	
		-- Delete old forecasts for this period. Long-term forecasts will be deleted also.
		--	Rerun usp_ARSLoader_ForecastRatingsMoreThan1Year
		DELETE FROM
			ratings
		WHERE
			rating_category_id = @Rating_category_id
			AND base_media_month_id = @baseMediaMonthID
			AND base_media_month_id <> forecast_media_month_id
			AND nielsen_network_id IN (
				SELECT nielsen_network_id FROM @nielsen_networks 
			)
			AND daypart_id IN (
				SELECT daypart_id FROM @dayparts
			);
	
		-- Save forecasts for this period
		WITH r (
			rating_category_id,
			base_media_month_id,
			forecast_media_month_id,
			nielsen_network_id, 
			audience_id,
			daypart_id,
			audience_usage,
			tv_usage			
		) AS (
			SELECT
				br.rating_category_id,
				@baseMediaMonthID base_media_month_id, 
				mm.id forecast_media_month_id,
				br.nielsen_network_id,
				br.audience_id,
				br.daypart_id, 
				br.audience_usage *
					case nn.network_rating_category_id
						when @particularCategoryID then
							case
								when rbm12.audience_usage + rbm24.audience_usage = 0
									 or
									 rbm12.audience_usage IS null
									 or
									 rbm24.audience_usage IS null then 
									case
										when gbm12.audience_usage + gbm24.audience_usage = 0 then NULL
										else
											(gfm12.audience_usage + gfm24.audience_usage)
											/
											(gbm12.audience_usage + gbm24.audience_usage)
									END 
								else
									(rfm12.audience_usage + rfm24.audience_usage) 
									/ 
									(rbm12.audience_usage + rbm24.audience_usage) 
							END
						when @generalCategoryID then
							case
								when rbm12.audience_usage + rbm24.audience_usage = 0
									 or
									 rbm12.audience_usage IS null
									 or
									 rbm24.audience_usage IS null then 
									case
										when gbm12.audience_usage + gbm24.audience_usage = 0 then NULL
										else
											(gfm12.audience_usage + gfm24.audience_usage)
											/
											(gbm12.audience_usage + gbm24.audience_usage)
									END 
								else
									(rfm12.audience_usage + rfm24.audience_usage) 
									/ 
									(rbm12.audience_usage + rbm24.audience_usage) 
							END
		--				when @generalCategoryID then
		--					case
		--						when gbm12.audience_usage + gbm24.audience_usage = 0 then NULL
		--						else
		--							(gfm12.audience_usage + gfm24.audience_usage)
		--							/
		--							(gbm12.audience_usage + gbm24.audience_usage)
		--					END 
						when @nonseasonalCategoryID then 1.0
						else NULL
					END audience_usage,
				br.tv_usage *
					case nn.network_rating_category_id
						when @particularCategoryID then
							case
								when rbm12.tv_usage + rbm24.tv_usage = 0
									 or
									 rbm12.tv_usage IS null
									 or
									 rbm24.tv_usage IS null then 
									case
										when gbm12.tv_usage + gbm24.tv_usage = 0 then NULL
										else
											(gfm12.tv_usage + gfm24.tv_usage)
											/
											(gbm12.tv_usage + gbm24.tv_usage)
									END 
								else
									(rfm12.tv_usage + rfm24.tv_usage) 
									/ 
									(rbm12.tv_usage + rbm24.tv_usage) 
							END
						when @generalCategoryID then
							case
								when rbm12.tv_usage + rbm24.tv_usage = 0
									 or
									 rbm12.tv_usage IS null
									 or
									 rbm24.tv_usage IS null then 
									case
										when gbm12.tv_usage + gbm24.tv_usage = 0 then NULL
										else
											(gfm12.tv_usage + gfm24.tv_usage)
											/
											(gbm12.tv_usage + gbm24.tv_usage)
									END 
								else
									(rfm12.tv_usage + rfm24.tv_usage) 
									/ 
									(rbm12.tv_usage + rbm24.tv_usage) 
							END
		--				when @generalCategoryID then
		--					case
		--						when gbm12.tv_usage + gbm24.tv_usage = 0 then NULL
		--						else
		--							(gfm12.tv_usage + gfm24.tv_usage)
		--							/
		--							(gbm12.tv_usage + gbm24.tv_usage)
		--					END 
						when @nonseasonalCategoryID then 1.0
						else NULL
					END tv_usage
			FROM
				average_ratings br (NOLOCK)
				JOIN nielsen_networks nn (NOLOCK) ON nn.id = br.nielsen_network_id
				JOIN media_months bmm (NOLOCK) ON bmm.id = br.base_media_month_id
				JOIN media_months fmm (NOLOCK) ON fmm.id = br.forecast_media_month_id
				JOIN media_months mm (NOLOCK) ON @lastForecastMonth > mm.start_date
					AND @firstForecastMonth < mm.end_date
				JOIN media_months fmm12 (NOLOCK) ON
					dateadd(year, -1, dateadd(day, datediff(day, mm.start_date, mm.end_date) / 2, mm.start_date))
						BETWEEN fmm12.start_date AND fmm12.end_date
				JOIN media_months fmm24 (NOLOCK) ON
					dateadd(year, -2, dateadd(day, datediff(day, mm.start_date, mm.end_date) / 2, mm.start_date))
						BETWEEN fmm24.start_date AND fmm24.end_date
				LEFT JOIN average_ratings gbm12 (NOLOCK) ON 
					gbm12.rating_category_id=br.rating_category_id
					AND gbm12.base_media_month_id=@baseM12MediaMonthID
					AND gbm12.forecast_media_month_id <> gbm12.base_media_month_id
					AND gbm12.nielsen_network_id=@generalNetworkID
					AND gbm12.audience_id=br.audience_id
					AND gbm12.daypart_id=br.daypart_id					
				LEFT JOIN average_ratings gbm24 (NOLOCK) ON 
					gbm24.rating_category_id=br.rating_category_id
					AND gbm24.base_media_month_id=@baseM24MediaMonthID
					AND gbm24.forecast_media_month_id <> gbm24.base_media_month_id
					AND gbm24.nielsen_network_id=@generalNetworkID
					AND gbm24.audience_id=br.audience_id
					AND gbm24.daypart_id=br.daypart_id
				LEFT JOIN average_ratings gfm12 (NOLOCK) ON br.rating_category_id = gfm12.rating_category_id
					AND gfm12.base_media_month_id=fmm12.id
					AND gfm12.forecast_media_month_id = gfm12.base_media_month_id
					AND gfm12.nielsen_network_id=@generalNetworkID
					AND gfm12.audience_id=br.audience_id
					AND gfm12.daypart_id=br.daypart_id
				LEFT JOIN average_ratings gfm24 (NOLOCK) ON
					gfm24.rating_category_id=br.rating_category_id
					AND gfm24.base_media_month_id=fmm24.id
					AND gfm24.forecast_media_month_id = gfm24.base_media_month_id
					AND gfm24.nielsen_network_id=@generalNetworkID
					AND gfm24.audience_id=br.audience_id
					AND gfm24.daypart_id=br.daypart_id
				LEFT JOIN average_ratings rbm12 (NOLOCK) ON
					rbm12.rating_category_id=br.rating_category_id
					AND rbm12.base_media_month_id=@baseM12MediaMonthID
					AND rbm12.nielsen_network_id=br.nielsen_network_id
					AND rbm12.audience_id=br.audience_id
					AND rbm12.daypart_id=br.daypart_id
				LEFT JOIN average_ratings rbm24 (NOLOCK) ON
					rbm24.rating_category_id=br.rating_category_id
					AND rbm24.base_media_month_id=@baseM24MediaMonthID
					AND rbm24.nielsen_network_id=br.nielsen_network_id
					AND rbm24.audience_id=br.audience_id
					AND rbm24.daypart_id=br.daypart_id					
				LEFT JOIN ratings rfm12 (NOLOCK) ON
					rfm12.rating_category_id=br.rating_category_id
					AND rfm12.base_media_month_id=fmm12.id
					AND rfm12.forecast_media_month_id=fmm12.id
					AND rfm12.nielsen_network_id=br.nielsen_network_id
					AND rfm12.audience_id=br.audience_id
					AND rfm12.daypart_id=br.daypart_id
				LEFT JOIN ratings rfm24 (NOLOCK) ON
					rfm24.rating_category_id=br.rating_category_id
					AND rfm24.base_media_month_id=fmm24.id
					AND rfm24.forecast_media_month_id=fmm24.id
					AND rfm24.nielsen_network_id=br.nielsen_network_id
					AND rfm24.daypart_id=br.daypart_id
					AND rfm24.audience_id=br.audience_id
			WHERE
				br.rating_category_id = @Rating_category_id
				AND br.base_media_month_id <> br.forecast_media_month_id
				AND @baseMediaMonthID = bmm.id
				AND @generalNetworkID <> br.nielsen_network_id
				AND br.daypart_id IN (
					SELECT daypart_id FROM @dayparts
				)
				AND br.nielsen_network_id IN (
					SELECT nielsen_network_id FROM @nielsen_networks
				)
		)
		-- Save forecasts for this period
		INSERT INTO ratings(
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
				rating_category_id,
				base_media_month_id,
				forecast_media_month_id,
				nielsen_network_id, 
				audience_id,
				daypart_id,
				audience_usage,
				tv_usage				
			FROM
				r
			WHERE
				audience_usage IS NOT NULL
				AND tv_usage IS NOT NULL
			ORDER BY
				base_media_month_id,
				forecast_media_month_id,
				nielsen_network_id, 
				audience_id,
				daypart_id;
				
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
