-- =============================================
-- Author:		David Sisson
-- Create date: 2008-08-15
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_update_general_ratings]
(
	@mediaMonth AS DATETIME,
	@ratingCategoryCode AS VARCHAR(15)
)
AS
BEGIN
	DECLARE
		@usNetworkID AS INT,
		@generalNetworkID AS INT,
		@mediaMonthID AS INT,
		@ratingCategoryId AS INT;

	SELECT
		@usNetworkID = id
	FROM
		nielsen_networks (NOLOCK)
	WHERE
		code = 'totUS';

	SELECT
		@generalNetworkID = id
	FROM
		nielsen_networks (NOLOCK)
	WHERE
		code = 'M_GEN';

	SELECT 
		@mediaMonthID = id
	FROM
		media_months (NOLOCK)
	WHERE
		@mediaMonth BETWEEN start_date AND end_date;

	SELECT 
		@ratingCategoryId = rc.id
	FROM 
		rating_categories rc (NOLOCK)
	WHERE 
		rc.code = @ratingCategoryCode;

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

	BEGIN TRY
		BEGIN TRANSACTION;
		-- Delete old ratings for the general pseudo-network FROM the average_ratings table
		print 'Deleting old ratings for the general pseudo-network FROM the average_ratings table...';
		DELETE FROM
			average_ratings
		WHERE
			rating_category_id = @ratingCategoryId
			AND base_media_month_id = @mediaMonthID
			AND forecast_media_month_id = @mediaMonthID
			AND nielsen_network_id = @generalNetworkID
			AND daypart_id IN (SELECT daypart_id FROM @dayparts);

		-- Insert new ratings for the general pseudo-network into the average_ratings table
		print 'Inserting new ratings for the general pseudo-network into the average_ratings table...';
		WITH
		wtgr(
			daypart_id,
			audience_id,
			audience_usage,
			tv_usage,
			rating_category_id
		)
		AS (
			SELECT
				r.daypart_id,
				r.audience_id,
				SUM(r.audience_usage * u.universe) audience_usage,
				SUM(r.tv_usage * u.universe) tv_usage,
				r.rating_category_id
			FROM
				ratings r (NOLOCK)
				JOIN universes u (NOLOCK) ON 
					r.rating_category_id = u.rating_category_id
					AND r.base_media_month_id = u.base_media_month_id
					AND r.forecast_media_month_id = u.forecast_media_month_id
					AND r.nielsen_network_id = u.nielsen_network_id
					AND r.audience_id = u.audience_id				
				JOIN nielsen_networks nn (NOLOCK) ON nn.id = r.nielsen_network_id
				JOIN network_rating_categories nrc (NOLOCK) ON nrc.id = nn.network_rating_category_id
			WHERE
				r.rating_category_id = @ratingCategoryId
				AND r.base_media_month_id = @mediaMonthID
				AND r.forecast_media_month_id = @mediaMonthID
				AND r.daypart_id IN (SELECT daypart_id FROM @dayparts)
				AND nrc.name = 'General'			
			GROUP BY 
				r.base_media_month_id,
				r.daypart_id,
				r.audience_id,
				r.rating_category_id
		)
		INSERT INTO average_ratings(
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
			@ratingCategoryId rating_category_id,
			@mediaMonthID base_media_month_id,
			@mediaMonthID forecast_media_month_id,
			@generalNetworkID nielsen_network_id,
			wtgr.audience_id,
			wtgr.daypart_id,
			wtgr.audience_usage / u.universe audience_usage,
			wtgr.tv_usage / u.universe tv_usage
		FROM
			wtgr
			JOIN universes u (NOLOCK) ON
				@ratingCategoryId = u.rating_category_id
				AND @mediaMonthID = u.base_media_month_id
				AND @mediaMonthID = u.forecast_media_month_id
				AND @usNetworkID = u.nielsen_network_id
				AND wtgr.audience_id = u.audience_id
		ORDER BY
			wtgr.daypart_id,
			wtgr.audience_id;

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
