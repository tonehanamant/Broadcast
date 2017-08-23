CREATE procedure [dbo].[usp_ARSLoader_PreCrunchUniverses]
	@codeMediaMonth as varchar(15),
	@codeRatingCategory as varchar(15)
AS
BEGIN
SET DATEFIRST 1

declare
	@idMediaMonth as int,
	@idRatingCategory as int,
	@textTimestamp as datetime;

BEGIN TRY
	BEGIN TRANSACTION;
	--if @codeRatingCategory <> 'NHIMIT' 
	--	begin
	--	raiserror(
	--		'Rating category must be NHIMIT--not %s.', 
	--		15, 
	--		1,
	--		@codeRatingCategory);
	--	end
	select @idMediaMonth = id from media_months (NOLOCK) where media_month = @codeMediaMonth;
	if @idMediaMonth is NULL 
		begin
		raiserror(
			'Month, %s, is not in the media_months table.', 
			15, 
			1,
			@codeMediaMonth);
		end
	select @idRatingCategory = id from rating_categories where code = @codeRatingCategory;
	if @idRatingCategory is NULL 
		begin
		raiserror(
			'Rating category, %s, is not in the rating_categories table.', 
			15, 
			1,
			@codeRatingCategory);
		end

	DELETE
		universes 
	WHERE 
		rating_category_id = @idRatingCategory
		AND base_media_month_id = @idMediaMonth;

	WITH monthly_universes (
		media_month_id,
		nielsen_network_id,
		audience_id,
		universe,
		rating_category_id
	)
	AS (
		SELECT
			mm.id [media_month_id],
			nn.id [nielsen_network_id],
			mua.audience_id [audience_id],
			dbo.agg_median( mua.universe ) [universe],
			mu.rating_category_id [rating_category_id]
		FROM
			media_months mm
			JOIN mit_universes mu ON mu.media_month_id=@idMediaMonth
				AND mu.rating_category_id=@idRatingCategory
				AND mm.start_date < mu.end_date AND mm.end_date > mu.start_date
			JOIN mit_universe_audiences mua ON mua.media_month_id=@idMediaMonth
				AND mu.id = mua.mit_universe_id
			JOIN nielsen_networks nn ON nn.id = mu.nielsen_network_id
		WHERE
			mm.id = @idMediaMonth
		GROUP BY
			nn.id,
			mm.id,
			mua.audience_id,
			mu.rating_category_id
	)

	INSERT INTO universes(
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
			nielsen_network_id,
			audience_id,
			universe
		FROM
			monthly_universes;
			
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
END
