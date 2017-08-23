CREATE PROCEDURE [rentrak].[usp_ARSLoader_PreCrunchUniversesForRentrak]
	@idMediaMonth AS INT,
	@idRatingCategory AS INT
AS
BEGIN
	DECLARE
		@textTimestamp AS VARCHAR(63),
		@textRows AS VARCHAR(63),
		@rows AS INT,
		@codeTimestampFormat AS INT;

	-- Enable dirty reads
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; 
	-- Don't auto report rows affected.
	SET NOCOUNT ON; 
	-- Make Monday the first day of the week.
	SET DATEFIRST 1;
	-- Use ODBC canonical (with milliseconds) format for timestamps <yyyy-mm-dd hh:mi:ss.mmm(24h)>
	SET @codeTimestampFormat = 121;

	BEGIN TRY
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), @codeTimestampFormat);
		RAISERROR(
			'%s - Processing actual universes data for %d''s %d book...', 
			0, 
			1, 
			@textTimestamp,
			@idRatingCategory,
			@idMediaMonth,
			@idRatingCategory) with nowait;
		
		DELETE FROM 
			universes 
		WHERE 
			base_media_month_id = @idMediaMonth
			AND 
			rating_category_id = @idRatingCategory;


		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), @codeTimestampFormat);
		RAISERROR(
			'%s - Filling universes...', 
			0, 
			1, 
			@textTimestamp) WITH NOWAIT;

		INSERT INTO
			universes(
				[rating_category_id],
				[base_media_month_id],
				[forecast_media_month_id],
				[nielsen_network_id],
				[audience_id],
				[universe]
			)
			SELECT
				@idRatingCategory rating_category_id,
				@idMediaMonth base_media_month_id,
				@idMediaMonth forecast_media_month_id,
				nnm.nielsen_network_id,
				am.audience_id,			
				AVG(vwrs.average_audience / vwrs.rating) * 100.0 universe
			FROM
				rentrak.rentrak.viewership vwrs
				JOIN audience_maps am ON am.map_set = 'Rentrak' AND am.map_value = vwrs.demographic_number
				JOIN nielsen_network_maps nnm ON nnm.map_set = 'Rentrak' AND nnm.map_value = vwrs.rentrak_id
			WHERE
				vwrs.media_month_id = @idMediaMonth
				AND vwrs.rating <> 0
			GROUP BY
				am.audience_id,
				nnm.nielsen_network_id
			ORDER BY
				am.audience_id,
				nnm.nielsen_network_id;
		
		-- Special case for All HHs
		INSERT INTO
			universes(
				[rating_category_id],
				[base_media_month_id],
				[forecast_media_month_id],
				[nielsen_network_id],
				[audience_id],
				[universe]
			)
			SELECT
				@idRatingCategory rating_category_id,
				@idMediaMonth base_media_month_id,
				@idMediaMonth forecast_media_month_id,
				nnm.nielsen_network_id,
				31,			
				(AVG(vwrs1.average_audience / vwrs1.rating) + AVG(vwrs2.average_audience / vwrs2.rating)) * 100.0 universe
			FROM 
				rentrak.rentrak.viewership vwrs1
				JOIN rentrak.rentrak.viewership vwrs2 ON vwrs2.media_month_id = vwrs1.media_month_id
					AND vwrs2.rentrak_id = vwrs1.rentrak_id
					AND vwrs2.time = vwrs1.time
				JOIN nielsen_network_maps nnm ON nnm.map_set = 'Rentrak' 
					AND nnm.map_value = vwrs1.rentrak_id
			WHERE
				vwrs1.demographic_number = '677'
				AND vwrs2.demographic_number = '678'
				AND vwrs1.media_month_id = @idMediaMonth
				AND	vwrs1.rating <> 0
				AND vwrs2.rating <> 0
			GROUP BY
				nnm.nielsen_network_id
			ORDER BY
				nnm.nielsen_network_id;

		SET @rows = @@ROWCOUNT;
		SET @textRows = LEFT(
			CONVERT(VARCHAR, CAST(@rows AS MONEY), 1), 
			LEN(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1))-3);
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), @codeTimestampFormat);
		RAISERROR(
			'%s - %s rows added to universes.', 
			0, 
			1, 
			@textTimestamp,
			@textRows) with nowait;

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
