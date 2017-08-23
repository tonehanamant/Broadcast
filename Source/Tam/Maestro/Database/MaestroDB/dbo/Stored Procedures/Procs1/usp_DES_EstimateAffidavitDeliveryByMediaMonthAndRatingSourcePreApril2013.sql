-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/20/2012
-- Modified:	1/22/2015 - 8315: modified logic which determines affidavit/audience combinations to calculate deliveyr for
-- Description:	
-- =============================================
-- usp_DES_EstimateAffidavitDeliveryByMediaMonthAndRatingSourcePreApril2013 NULL,372,3
CREATE PROCEDURE [dbo].[usp_DES_EstimateAffidavitDeliveryByMediaMonthAndRatingSourcePreApril2013]
	@idAffidavitDeliveryRun INT,
	@idMediaMonth INT,
	@idRatingSource INT
AS
BEGIN
	SET NOCOUNT ON; -- Used for performance AND making sure we don't send back unneeded information.
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

	BEGIN TRY
		DECLARE @ErrorNumber INT;
		DECLARE @ErrorSeverity INT;
		DECLARE @ErrorState INT;
		DECLARE @ErrorProcedure NVARCHAR(200) ;
		DECLARE @ErrorLine INT;
		DECLARE @ErrorMessage NVARCHAR(4000);
		
		DECLARE
			@idHHAudience INT,
			@textTimestamp VARCHAR(63),
			@textRows VARCHAR(63),
			@codeMediaMonth VARCHAR(15),
			@codeRatingSource VARCHAR(63),
			@rows INT,
			@timestamp DATETIME,
			@subrowcount INT,
			@remainingrowcount INT,
			@batchSize INT,
			@rating_category_group_id TINYINT;
			
		SET @batchSize = 500000;

		IF @idAffidavitDeliveryRun  IS NOT NULL
			UPDATE dbo.affidavit_delivery_runs SET date_started=GETDATE(),date_last_updated=GETDATE() WHERE id=@idAffidavitDeliveryRun
			
		SELECT @codeMediaMonth = mm.media_month FROM media_months mm WHERE mm.id = @idMediaMonth
		SELECT @codeRatingSource = rs.code FROM rating_sources rs WHERE rs.id = @idRatingSource
		SELECT @rating_category_group_id = dbo.GetRatingCategoryGroupIdOfRatingsSource(@idRatingSource)
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		SET @idHHAudience = dbo.GetIDFromAudienceString('hh');

		RAISERROR('%s - Estimating %s delivery for %s...', 0, 1, @textTimestamp, @codeRatingSource, @codeMediaMonth) WITH NOWAIT;
		CREATE TABLE #temp_done_affidavit_processing (affidavit_id BIGINT NOT NULL, audience_id INT NOT NULL);
		ALTER TABLE #temp_done_affidavit_processing ADD CONSTRAINT [IX_TempDoneAffidavitProcessing] PRIMARY KEY CLUSTERED 
		(
			[affidavit_id] ASC,
			[audience_id] ASC
		);

		SET @timestamp = GETDATE();
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Filling #temp_done_affidavit_processing...', 0, 1, @textTimestamp) WITH NOWAIT;
		
		
		/*******************************************************************************************************************
		Get all AFFIDAVITS and AUDIENCES already calculated for the month for this RATINGSOURCE
		*******************************************************************************************************************/
		INSERT INTO #temp_done_affidavit_processing(affidavit_id, audience_id)
			SELECT
				ad.affidavit_id,
				ad.audience_id
			FROM
				affidavit_deliveries ad	(NOLOCK)
			WHERE
				ad.media_month_id=@idMediaMonth AND ad.rating_source_id=@idRatingSource
			GROUP BY
				ad.affidavit_id,
				ad.audience_id;
			
		SET @rows = @@ROWCOUNT;
		IF @idAffidavitDeliveryRun IS NOT NULL
			UPDATE dbo.affidavit_delivery_runs SET date_last_updated=GETDATE(), total_done_affidavits=@rows, time_to_get_done_affidavits=DATEDIFF(second, @timestamp, GETDATE()) WHERE id=@idAffidavitDeliveryRun
		SET @textRows = LEFT(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1), LEN(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1))-3);
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - %s rows added to #temp_done_affidavit_processing.', 0, 1, @textTimestamp, @textRows) WITH NOWAIT;


		CREATE TABLE #temp_needed_affidavit_processing(affidavit_id BIGINT, audience_id INT);
		CREATE INDEX IX_TempNeededAffidavitProcessing ON #temp_needed_affidavit_processing (affidavit_id, audience_id);

		SET @timestamp = GETDATE();
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Filling #temp_needed_affidavit_processing...', 0, 1, @textTimestamp) WITH NOWAIT;


		/*******************************************************************************************************************
		Get all AFFIDAVITS and AUDIENCES we need to caluclate based on married ISC proposal data MINUS everything we already have calculated.
		*******************************************************************************************************************/
		INSERT INTO #temp_needed_affidavit_processing(affidavit_id, audience_id)
			SELECT DISTINCT
				pa.audience_id,
				a.id 'affidavit_id'
			FROM
				proposals p
				JOIN proposal_materials pm	 ON pm.proposal_id=p.id
				JOIN material_revisions mr	 ON mr.revised_material_id=pm.material_id
				JOIN proposal_audiences pa	 ON pa.proposal_id IN (p.id, p.original_proposal_id)
				JOIN affidavits a			 ON a.media_month_id=@idMediaMonth AND a.material_id=mr.original_material_id
			WHERE
				p.posting_media_month_id=@idMediaMonth
				AND (
					-- we want to calculate MIT Live delivery for all posting plans regardless of inputted ratings source
					@idRatingSource=1
					-- the posting plan has the rating source being run
					OR p.rating_source_id=@idRatingSource
					-- all posting plans in a post which has the rating source being run
					OR p.id IN (
						SELECT DISTINCT
							p2.id 
						FROM 
							tam_post_proposals tpp
							JOIN tam_posts tp ON tp.id=tpp.tam_post_id
								AND tp.rating_source_id=@idRatingSource
							JOIN proposals p2 ON p2.id=tpp.posting_plan_proposal_id
								AND p2.posting_media_month_id=@idMediaMonth
					)
				)

			EXCEPT

			SELECT
				affidavit_id, 
				audience_id
			FROM
				#temp_needed_affidavit_processing;
				
		SET @rows = @@ROWCOUNT;
		SET @textRows = LEFT(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1), LEN(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1))-3);
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - %s rows added to #temp_needed_affidavit_processing FROM proposals with MARRIED ISCIs.', 0, 1, @textTimestamp, @textRows) WITH NOWAIT;


		/*******************************************************************************************************************
		Get all AFFIDAVITS and AUDIENCES we need to caluclate based on non-married ISCI proposal data MINUS everything we already have calculated.
		*******************************************************************************************************************/
		INSERT INTO #temp_needed_affidavit_processing(affidavit_id, audience_id)
			SELECT DISTINCT
				pa.audience_id,
				a.id 'affidavit_id'
			FROM
				proposals p
				JOIN proposal_materials pm	 ON pm.proposal_id=p.id
				JOIN proposal_audiences pa	 ON pa.proposal_id IN (p.id, p.original_proposal_id)
				JOIN affidavits a			 ON a.media_month_id=@idMediaMonth AND a.material_id=pm.material_id
			WHERE
				p.posting_media_month_id=@idMediaMonth
				AND (
					-- we want to calculate MIT Live delivery for all posting plans regardless of inputted ratings source
					@idRatingSource=1
					-- the posting plan has the rating source being run
					OR p.rating_source_id=@idRatingSource
					-- all posting plans in a post which has the rating source being run
					OR p.id IN (
						SELECT DISTINCT
							p2.id 
						FROM 
							tam_post_proposals tpp
							JOIN tam_posts tp ON tp.id=tpp.tam_post_id
								AND tp.rating_source_id=@idRatingSource
							JOIN proposals p2 ON p2.id=tpp.posting_plan_proposal_id
								AND p2.posting_media_month_id=@idMediaMonth
					)
				)

			EXCEPT

			SELECT
				affidavit_id, 
				audience_id
			FROM
				#temp_needed_affidavit_processing;
				
		SET @rows = @@ROWCOUNT;
		SET @textRows = LEFT(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1), LEN(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1))-3);
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - %s rows added to #temp_needed_affidavit_processing FROM proposals with NON-MARRIED ISCIs.', 0, 1, @textTimestamp, @textRows) WITH NOWAIT;
		IF @idAffidavitDeliveryRun IS NOT NULL
			UPDATE dbo.affidavit_delivery_runs SET date_last_updated=GETDATE(), total_needed_affidavits=(SELECT COUNT(*) FROM #temp_needed_affidavit_processing), time_to_get_needed_affidavits=DATEDIFF(second, @timestamp, GETDATE()) WHERE id=@idAffidavitDeliveryRun
		
		
		/*******************************************************************************************************************
		Now that we know all the AFFIDAVIT/AUDIENCE combinations we NEED, and all that we have DONE, EXCEPT the two to get REMAINING
		*******************************************************************************************************************/
		CREATE TABLE #temp_remaining_affidavit_processing(affidavit_id BIGINT NOT NULL, audience_id INT NOT NULL);
		ALTER TABLE #temp_remaining_affidavit_processing ADD CONSTRAINT [IX_TempRemainingAffidavitProcessing] PRIMARY KEY CLUSTERED 
		(
			[affidavit_id] ASC,
			[audience_id] ASC
		);

		SET @timestamp = GETDATE();
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Filling #temp_remaining_affidavit_processing...', 0, 1, @textTimestamp) WITH NOWAIT;

		INSERT INTO #temp_remaining_affidavit_processing(affidavit_id, audience_id)
			SELECT
				nap.affidavit_id,
				nap.audience_id
			FROM
				#temp_needed_affidavit_processing nap
			EXCEPT
			SELECT
				affidavit_id,
				audience_id
			FROM
				#temp_done_affidavit_processing;
		

		SET @rows = @@ROWCOUNT;
		SET @textRows = LEFT(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1), LEN(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1))-3);
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - %s rows added to #temp_remaining_affidavit_processing.', 0, 1, @textTimestamp, @textRows) WITH NOWAIT;
		IF @idAffidavitDeliveryRun IS NOT NULL
			UPDATE dbo.affidavit_delivery_runs SET date_last_updated=GETDATE(), total_remaining_affidavits=(SELECT COUNT(*) FROM #temp_remaining_affidavit_processing), time_to_get_remaining_affidavits=DATEDIFF(second, @timestamp, GETDATE()) WHERE id=@idAffidavitDeliveryRun

		-- now that we've calculated the REMAINING these two temporary tables are no longer needed so get rid of them
		IF OBJECT_ID('tempdb..#temp_needed_affidavit_processing') > 0 
			DROP TABLE #temp_needed_affidavit_processing;
		IF OBJECT_ID('tempdb..#temp_done_affidavit_processing') > 0 
			DROP TABLE #temp_done_affidavit_processing;


		/*******************************************************************************************************************
		Get Nielsen Network Rating Map
		*******************************************************************************************************************/
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Filling #temp_nielsen_network_ratings_map...', 0, 1, @textTimestamp) WITH NOWAIT;

		CREATE TABLE #temp_nielsen_network_ratings_map(network_id INT, substitution_category_id INT, nielsen_network_id INT, weight FLOAT,start_date DATETIME, end_date DATETIME, source VARCHAR(20), rating_category_group_id TINYINT);		
		CREATE INDEX IX_TempNielsenNetworkRatingsMap ON #temp_nielsen_network_ratings_map (substitution_category_id, network_id, start_date);
		INSERT INTO #temp_nielsen_network_ratings_map(network_id,substitution_category_id,nielsen_network_id,weight,start_date,end_date,source,rating_category_group_id)
			SELECT
				network_id, 
				substitution_category_id, 
				nielsen_network_id, 
				weight,
				start_date, 
				end_date, 
				source,
				rating_category_group_id
			FROM
				uvw_posting_network_2_nielsen_network_ratings_map
		
		SET @rows = @@ROWCOUNT;
		SET @textRows = LEFT(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1), LEN(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1))-3);	
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - %s rows added to ##temp_nielsen_network_ratings_map.', 0, 1, @textTimestamp, @textRows) WITH NOWAIT;



		/*******************************************************************************************************************
		Calculate Deliveries
		*******************************************************************************************************************/
		SET @rows = 0;
		SET @timestamp = GETDATE();
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Filling affidavit_deliveries at %d intervals...', 0, 1, @textTimestamp, @batchSize) WITH NOWAIT;
		
		CREATE TABLE #temp_batch_affidavit_processing(affidavit_id BIGINT, audience_id INT);
		CREATE INDEX IX_TempBatchAffidavitProcessing ON #temp_batch_affidavit_processing (affidavit_id, audience_id);

		WHILE 1 = 1
		BEGIN
			INSERT INTO #temp_batch_affidavit_processing (affidavit_id, audience_id)
				SELECT
					TOP(@batchSize) affidavit_id, audience_id
				FROM
					#temp_remaining_affidavit_processing
					
			BEGIN TRY
				BEGIN TRANSACTION T1;

				INSERT INTO affidavit_deliveries WITH (TABLOCK) (media_month_id, rating_source_id, audience_id, affidavit_id, audience_usage, universe)
					SELECT
						@idMediaMonth,
						@idRatingSource,
						rap.audience_id,
						rap.affidavit_id,						
						SUM(viewership.usage) * nnrm.weight audience_usage,
						SUM(viewers.universe) * nnum.weight universe
					FROM
						#temp_batch_affidavit_processing rap
						JOIN affidavits a ON
							a.media_month_id=@idMediaMonth 
							AND 
							a.id=rap.affidavit_id
						JOIN #temp_nielsen_network_ratings_map nnrm ON
							1=nnrm.substitution_category_id
							AND
							nnrm.network_id=a.network_id
							AND
							a.air_date BETWEEN nnrm.start_date AND nnrm.end_date
							AND
							(nnrm.rating_category_group_id=@rating_category_group_id OR nnrm.rating_category_group_id IS NULL)
						JOIN audience_audiences aa ON
							aa.custom_audience_id=rap.audience_id
							AND aa.rating_category_group_id=@rating_category_group_id
						JOIN rating_source_rating_categories rsrc ON 
							rsrc.rating_source_id=@idRatingSource
						JOIN mit_ratings mr ON
							mr.media_month_id=@idMediaMonth
							AND
							mr.nielsen_network_id=nnrm.nielsen_network_id
							AND
							mr.rating_date=a.air_date
							AND
							a.air_time BETWEEN mr.start_time AND mr.end_time
							AND
							mr.rating_category_id=rsrc.rating_category_id
						JOIN mit_person_audiences viewership ON
							viewership.media_month_id=@idMediaMonth
							AND
							viewership.mit_rating_id=mr.id
							AND
							viewership.audience_id=aa.rating_audience_id
						JOIN #temp_nielsen_network_ratings_map nnum ON
							2=nnum.substitution_category_id
							AND
							a.network_id=nnum.network_id
							AND
							a.air_date BETWEEN nnum.start_date AND nnum.end_date
							AND
							(nnum.rating_category_group_id=@rating_category_group_id OR nnum.rating_category_group_id IS NULL)
						JOIN mit_universes mu ON
							mu.media_month_id=@idMediaMonth
							AND
							mu.rating_category_id=rsrc.rating_category_id
							AND
							nnum.nielsen_network_id = mu.nielsen_network_id
							AND
							a.air_date BETWEEN mu.start_date AND mu.end_date
						JOIN mit_universe_audiences viewers ON
							viewers.media_month_id=@idMediaMonth
							AND
							viewers.mit_universe_id=mu.id
							AND
							viewers.audience_id=aa.rating_audience_id
					GROUP BY
						rap.affidavit_id, 
						rap.audience_id,
						nnrm.weight,
						nnum.weight;
			
				SET @subrowcount = @@ROWCOUNT
				SET @rows = @rows + @subrowcount;
				SET @textRows = LEFT(CONVERT(VARCHAR, CAST(@subrowcount AS MONEY), 1), LEN(CONVERT(VARCHAR, CAST(@subrowcount AS MONEY), 1))-3);
				SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
				SET @remainingrowcount = (SELECT COUNT(*) FROM #temp_remaining_affidavit_processing) - @batchSize
				IF @remainingrowcount < 0
					SET @remainingrowcount = 0
				RAISERROR('%s - %s total rows added to affidavit_deliveries with %d remaining...', 0, 1, @textTimestamp, @textRows, @remainingrowcount) WITH NOWAIT;
				IF @idAffidavitDeliveryRun IS NOT NULL
					UPDATE dbo.affidavit_delivery_runs SET date_last_updated=GETDATE(), num_processed=@rows, num_remaining=@remainingrowcount WHERE id=@idAffidavitDeliveryRun

				COMMIT TRANSACTION T1;
			END TRY
			BEGIN CATCH
				ROLLBACK TRANSACTION T1;
				
				SELECT 
					@ErrorNumber = ERROR_NUMBER(),
					--@ErrorSeverity = ERROR_SEVERITY(),
					@ErrorState = ERROR_STATE(),
					@ErrorProcedure = isnull(ERROR_PROCEDURE(), 'N/A'),
					@ErrorLine = ERROR_LINE(),
					@ErrorMessage = N'Error %d, Level %d, State %d, Procedure %s, Line %d, ' + 'Message: ' + ERROR_MESSAGE();
				
				RAISERROR(
					@ErrorMessage, 
					0, 
					1,               
					@ErrorNumber,    -- parameter: original error number.
					@ErrorSeverity,  -- parameter: original error severity.
					@ErrorState,     -- parameter: original error state.
					@ErrorProcedure, -- parameter: original error procedure name.
					@ErrorLine       -- parameter: original error line number.
				);
			END CATCH;
			
			-- remove records just processed
			DELETE
				#temp_remaining_affidavit_processing
			FROM
				#temp_remaining_affidavit_processing rap
				JOIN #temp_batch_affidavit_processing bap ON bap.affidavit_id=rap.affidavit_id AND bap.audience_id=rap.audience_id
			-- clear processing table for next batch
			DELETE FROM #temp_batch_affidavit_processing;

			IF @remainingrowcount <= 0 BREAK
		END
		
		SET @textRows = LEFT(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1), LEN(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1))-3);
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Estimating delivery for %s is done. %s rows added to affidavit_deliveries table.', 0, 1, @textTimestamp, @codeMediaMonth, @textRows) WITH NOWAIT;
		IF @idAffidavitDeliveryRun IS NOT NULL
			UPDATE dbo.affidavit_delivery_runs SET date_last_updated=GETDATE(), date_completed=GETDATE(), time_to_calculate_deliveries=DATEDIFF(second, @timestamp, GETDATE()) WHERE id=@idAffidavitDeliveryRun
	END TRY
	BEGIN CATCH
		SELECT 
			@ErrorNumber = ERROR_NUMBER(),
			@ErrorSeverity = ERROR_SEVERITY(),
			@ErrorState = ERROR_STATE(),
			@ErrorProcedure = isnull(ERROR_PROCEDURE(), 'N/A'),
			@ErrorLine = ERROR_LINE(),
			@ErrorMessage = N'Error %d, Level %d, State %d, Procedure %s, Line %d, ' + 'Message: ' + ERROR_MESSAGE();
		
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

	SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
	RAISERROR('%s - Dropping temporary tables...', 0, 1, @textTimestamp) WITH NOWAIT;

	IF OBJECT_ID('tempdb..#temp_nielsen_network_ratings_map') > 0 
		DROP TABLE #temp_nielsen_network_ratings_map;
	IF OBJECT_ID('tempdb..#temp_remaining_affidavit_processing') > 0 
		DROP TABLE #temp_remaining_affidavit_processing;
	IF OBJECT_ID('tempdb..#temp_batch_affidavit_processing') > 0 
		DROP TABLE #temp_batch_affidavit_processing;
		
	SET NOCOUNT OFF;
END
