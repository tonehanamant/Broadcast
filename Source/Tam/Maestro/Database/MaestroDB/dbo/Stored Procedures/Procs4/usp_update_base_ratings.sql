-- =============================================
-- Author:		David Sisson
-- Create date: 8/11/2008
-- Description:	Updates the base ratings
-- =============================================
CREATE PROCEDURE [dbo].[usp_update_base_ratings]
(
	@baseMonth AS DateTime,
	@nMonthsToAverage AS INT,
	@ratingCategoryCode varchar(15)
)
AS
BEGIN
	DECLARE
		@month AS DATETIME,
		@mediaMonthID AS INT,
		@baseMediaMonthID AS INT,
		@forecastMediaMonthID AS INT,
		@cMonth AS INT,
		@ratingCategoryId AS INT;
	DECLARE 
		@months_in_average AS TABLE (
			ordinal INT,
			media_month_id INT
		);

	SELECT
		@month = dateadd(day,14,dbo.Period2FirstOfMonth(bmm.media_month)),
		@baseMediaMonthID = bmm.id
	FROM
		media_months bmm
	WHERE
		@baseMonth BETWEEN bmm.start_date AND bmm.end_date;

	SET @mediaMonthID = @baseMediaMonthID;
	SET @cMonth = 0;
	while ( 1 = 1 ) BEGIN
		SET @cMonth = @cMonth + 1;
		IF ( @mediaMonthID IS NOT NULL ) BEGIN
			INSERT INTO
				@months_in_average( ordinal, media_month_id )
			values
				( @cMonth, @mediaMonthID );

		END;
		
		IF ( @cMonth >= @nMonthsToAverage ) BEGIN
			SET @forecastMediaMonthID = @mediaMonthID;
			break;
		END;

		SET @month = dateadd( month, -1, @month );

		SELECT
			@mediaMonthID = mm.id
		FROM
			media_months mm
		WHERE
			@month BETWEEN mm.start_date AND mm.end_date;
	END;

	SELECT @ratingCategoryId = id 
	FROM rating_categories 
	WHERE code = @ratingCategoryCode;

	DECLARE @dayparts TABLE(
		daypart_id INT
	);

	INSERT INTO 
		@dayparts(daypart_id)
		SELECT DISTINCT
			daypart_id
		FROM 
			daypart_maps dpm
		WHERE
			dpm.map_set IN ('Ratings', 'FM_nDyptNum');

	DECLARE @nielsen_networks TABLE(
		nielsen_network_id INT
	);

	INSERT INTO 
		@nielsen_networks(nielsen_network_id)
		SELECT
			id
		FROM 
			nielsen_networks nn;
	--	WHERE
	--		nn.code IN('adsm', 'nan', 'nick', 'toon');
	
	BEGIN TRY
		BEGIN TRANSACTION;
		-- Delete old base ratings FROM the average_ratings table
		print 'Deleting old base ratings FROM the average_ratings table...';
		DELETE
			average_ratings
		FROM
			average_ratings ar
			JOIN media_months bmm ON bmm.id = ar.base_media_month_id
			JOIN media_months fmm ON fmm.id = ar.forecast_media_month_id
		WHERE
			ar.rating_category_id = @ratingCategoryId
			AND ar.base_media_month_id = @baseMediaMonthID
			AND ar.nielsen_network_id IN (SELECT nielsen_network_id FROM @nielsen_networks)
			AND ar.daypart_id IN (SELECT daypart_id FROM @dayparts)
			AND bmm.start_date > fmm.start_date;
	
		-- Insert new base ratings into the average_ratings table
		print 'Inserting new base ratings into the average_ratings table...';
		INSERT INTO average_ratings(rating_category_id, base_media_month_id, forecast_media_month_id, nielsen_network_id, audience_id, daypart_id, audience_usage, tv_usage)
			SELECT
				@ratingCategoryId,
				@baseMediaMonthID,
				@forecastMediaMonthID,
				r.nielsen_network_id,
				r.audience_id,
				r.daypart_id,
				AVG(r.audience_usage),
				AVG(r.tv_usage)
			FROM
				ratings r (NOLOCK)
				JOIN @months_in_average mia ON mia.media_month_id = r.base_media_month_id
					AND mia.media_month_id = r.forecast_media_month_id
			WHERE
				r.rating_category_id = @ratingCategoryId
				AND r.nielsen_network_id IN (SELECT nielsen_network_id FROM @nielsen_networks)
				AND r.daypart_id IN (SELECT daypart_id FROM @dayparts)
			GROUP BY
				r.nielsen_network_id,
				r.daypart_id,
				r.audience_id;
				
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
