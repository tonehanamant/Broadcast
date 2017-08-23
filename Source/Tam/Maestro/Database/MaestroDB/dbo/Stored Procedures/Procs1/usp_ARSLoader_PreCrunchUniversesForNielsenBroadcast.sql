CREATE PROCEDURE [dbo].[usp_ARSLoader_PreCrunchUniversesForNielsenBroadcast]
	@codeMediaMonth as varchar(15),
	@codeRatingCategory as varchar(15)
AS
BEGIN
SET DATEFIRST 1

declare
	@idMediaMonth as int,
	@idRatingCategory as int,
	@textTimestamp as datetime,
	@mediaMonthStartDate AS DATETIME;

BEGIN TRY
	BEGIN TRANSACTION;

	SELECT 
		@idMediaMonth = id, 
		@mediaMonthStartDate = start_date 
	FROM 
		media_months (NOLOCK)
	WHERE 
		media_month = @codeMediaMonth;

	if @idMediaMonth is NULL 
	begin
		raiserror(
			'Month, %s, is not in the media_months table.', 
			15, 
			1,
			@codeMediaMonth);
	end

	SELECT 
		@idRatingCategory = id 
	FROM 
		rating_categories (NOLOCK)
	WHERE 
		code = @codeRatingCategory;

	if @idRatingCategory is NULL 
	begin
		raiserror(
			'Rating category, %s, is not in the rating_categories table.', 
			15, 
			1,
			@codeRatingCategory);
	end;

	DELETE
		universes 
	WHERE 
		rating_category_id = @idRatingCategory
		AND base_media_month_id = @idMediaMonth;

	WITH monthly_universes 
	(
		rating_category_id,
		media_month_id,
		nielsen_network_id,
		audience_id,
		universe		
	)
	AS
	(
		SELECT
			mu.rating_category_id [rating_category_id],
			@idMediaMonth [media_month_id],
			mu.nielsen_network_id [nielsen_network_id],
			mua.audience_id [audience_id],
			dbo.agg_median( mua.universe ) [universe]			
		FROM
			mit_universes mu (NOLOCK)
			JOIN mit_universe_audiences mua (NOLOCK) ON mu.id=mua.mit_universe_id
		WHERE
			mu.rating_category_id = @idRatingCategory
			AND mu.media_month_id IN
			(
				SELECT TOP 1 
					mm.id
				FROM 
					mit_universes mu (NOLOCK)
				JOIN media_months mm (NOLOCK) ON mm.id = mu.media_month_id
				WHERE 
					mu.rating_category_id = @idRatingCategory
					AND mm.start_date <= @mediaMonthStartDate
				ORDER BY 
					mm.start_date DESC
			)
		GROUP BY
			mu.nielsen_network_id,
			mu.media_month_id,
			mua.audience_id,
			mu.rating_category_id
	),
	
	networks 
	(
		id
	)
	AS 
	(
		SELECT DISTINCT 
			mr.nielsen_network_id
		FROM 
			mit_ratings mr (NOLOCK)
		WHERE 
			mr.rating_category_id = @idRatingCategory
			AND mr.media_month_id = @idMediaMonth
	)
	
	INSERT INTO universes
	(
		rating_category_id,
		base_media_month_id,
		forecast_media_month_id,
		nielsen_network_id,
		audience_id,
		universe
	)
	SELECT
		rating_category_id,
		media_month_id,
		media_month_id,
		networks.id,
		audience_id,
		universe
	FROM
		monthly_universes
		CROSS JOIN networks
	ORDER BY 
		networks.id, 
		audience_id

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
	
	IF (XACT_STATE()) = -1-- If -1, the transaction is uncommittable
		BEGIN
		PRINT
			N'The transaction is in an uncommittable state. ' +
			'Rolling back transaction.'
		ROLLBACK TRANSACTION;
		END;
	ELSE IF (XACT_STATE()) = 1-- If 1, the transaction is committable.
		BEGIN
		PRINT
			N'The transaction is committable. ' +
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
END;
