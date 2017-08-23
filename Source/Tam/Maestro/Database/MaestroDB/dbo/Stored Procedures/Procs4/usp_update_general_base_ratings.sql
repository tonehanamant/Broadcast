-- =============================================
-- Author:		David Sisson
-- Create date: 2008-08-11
-- Description:	Update general base ratings
-- =============================================
CREATE PROCEDURE [dbo].[usp_update_general_base_ratings]
(
	@baseMonth AS DateTime,
	@nMonthsToAverage AS INT,
	@ratingCategoryCode AS VARCHAR(15)
)
AS
BEGIN
	DECLARE
		@generalNetworkID AS INT,
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
		@generalNetworkID = id
	FROM
		nielsen_networks
	WHERE
		code = 'M_GEN';

	SELECT @ratingCategoryId = id
	FROM rating_categories
	WHERE code = @ratingCategoryCode;

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

	BEGIN TRY
		BEGIN TRANSACTION;
		-- Delete old base ratings for the general pseudo-network FROM the average_ratings table
		print 'Deleting old base ratings for the general pseudo-network FROM the average_ratings table...';
		
		DELETE
			average_ratings
		FROM
			average_ratings ar
			JOIN media_months bmm ON bmm.id = ar.base_media_month_id
			JOIN media_months fmm ON fmm.id = ar.forecast_media_month_id
		WHERE
			ar.rating_category_id = @ratingCategoryId
			AND ar.base_media_month_id = @baseMediaMonthID
			AND ar.nielsen_network_id = @generalNetworkID
			AND daypart_id IN (SELECT daypart_id FROM @dayparts)
			AND bmm.start_date > fmm.start_date
	
		-- Insert new base ratings for the general pseudo-network into the average_ratings table
		print 'Inserting new base ratings for the general pseudo-network into the average_ratings table...';
		INSERT INTO average_ratings(rating_category_id, base_media_month_id, forecast_media_month_id, nielsen_network_id, audience_id, daypart_id, audience_usage, tv_usage)
			SELECT
				@ratingCategoryId,
				@baseMediaMonthID,
				@forecastMediaMonthID,
				ar.nielsen_network_id,
				ar.audience_id,
				ar.daypart_id,
				AVG(ar.audience_usage),
				AVG(ar.tv_usage)
			FROM 
				average_ratings ar (NOLOCK)
				JOIN @months_in_average mia ON mia.media_month_id = ar.base_media_month_id
					AND mia.media_month_id = ar.forecast_media_month_id
			WHERE
				ar.rating_category_id = @ratingCategoryId
				AND ar.nielsen_network_id = @generalNetworkID
				AND ar.daypart_id IN (SELECT daypart_id FROM @dayparts)
			GROUP BY
				ar.nielsen_network_id,
				ar.daypart_id,
				ar.audience_id;
			
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
