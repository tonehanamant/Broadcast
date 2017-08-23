-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/23/2013
-- Modified:	1/22/2015 - 8315: modified logic which determines affidavit/audience combinations to calculate deliveyr for
--				5/15/2015 - Updated to support NULL adjusted_air_date and adjusted_air_time
-- Description:	Calculates RENTRAK (rentrak.viewership) delivery for affidavits.
--				This takes into account time zone shifts by zone and has special logic to check if a zone has become rated
--				in which case it will cease using a network substitution.
--				This is designed to post affidavits for the rentrak.viewership table only.
--				Note @idRatingSource is translated into rating categories.
-- =============================================
-- usp_DES_EstimateRentrakAffidavitDeliveryByMediaMonthAndRatingSourceWithTimeZoneAdjustment NULL,388,5,NULL
CREATE PROCEDURE [dbo].[usp_DES_EstimateRentrakAffidavitDeliveryByMediaMonthAndRatingSourceWithTimeZoneAdjustment]
	@idAffidavitDeliveryRun INT, -- optional
	@idMediaMonth INT,
	@idRatingSource INT,
	@stopHour INT -- optional
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

	BEGIN TRY
		DECLARE @ErrorNumber INT;
		DECLARE @ErrorSeverity INT;
		DECLARE @ErrorState INT;
		DECLARE @ErrorProcedure NVARCHAR(200);
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
			@iterator_date DATETIME, 
			@start_date DATETIME, 
			@end_date DATETIME,
			@subrowcount INT,
			@remainingrowcount INT,
			@batchSize INT,
			@monthYear INT,
			@currentHour INT,
			@rating_category_group_id TINYINT;
			
		SELECT @start_date=mm.start_date, @end_date=mm.end_date FROM dbo.media_months mm WHERE mm.id=@idMediaMonth
		SET @batchSize = 500000;

		IF @idAffidavitDeliveryRun  IS NOT NULL
			UPDATE dbo.affidavit_delivery_runs SET date_started=GETDATE(),date_last_updated=GETDATE() WHERE id=@idAffidavitDeliveryRun
		
		SELECT @codeMediaMonth = mm.media_month, @monthYear = mm.[year] FROM dbo.media_months mm WHERE mm.id = @idMediaMonth
		SELECT @codeRatingSource = rs.code FROM rating_sources rs WHERE rs.id = @idRatingSource
		SELECT @rating_category_group_id = dbo.GetRatingCategoryGroupIdOfRatingsSource(@idRatingSource)
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		SET @idHHAudience = dbo.GetIDFromAudienceString('hh');

		RAISERROR('%s - Estimating %s delivery for %s...', 0, 1, @textTimestamp, @codeRatingSource, @codeMediaMonth) WITH NOWAIT;
		IF OBJECT_ID('.af_delivery_queue') IS NOT NULL
			DROP TABLE dbo.af_delivery_queue;

		-- update total_done_affidavits
		IF @idAffidavitDeliveryRun IS NOT NULL
		BEGIN
			SELECT @rows = COUNT(1) FROM affidavit_deliveries ad WHERE ad.media_month_id=@idMediaMonth AND ad.rating_source_id=@idRatingSource;

			UPDATE dbo.affidavit_delivery_runs SET 
				date_last_updated=GETDATE(), 
				total_done_affidavits=@rows, 
				time_to_get_done_affidavits=0 
			WHERE 
				id=@idAffidavitDeliveryRun;
		END
	
		-- clear out previous rentrak viewership data
		TRUNCATE TABLE rentrak_viewership;

		CREATE TABLE dbo.af_delivery_queue (audience_id INT NOT NULL, affidavit_id BIGINT NOT NULL);
		CREATE UNIQUE CLUSTERED INDEX IX_af_delivery_queue ON dbo.af_delivery_queue ([audience_id] ASC, [affidavit_id] ASC);
 
		SET @timestamp = GETDATE();
 	
 		/*******************************************************************************************************************
		STEP 1 OF 5: Queue Affidavits to Calculate Delivery For
		*******************************************************************************************************************/
		-- MARRIED ISCI's
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Filling dbo.af_delivery_queue with married ISCIs...', 0, 1, @textTimestamp) WITH NOWAIT;
		IF @idAffidavitDeliveryRun IS NOT NULL 
		BEGIN
			WAITFOR DELAY '000:00:00.500'
			INSERT INTO dbo.affidavit_delivery_run_messages SELECT @idAffidavitDeliveryRun,GETDATE(),'Filling dbo.af_delivery_queue with married ISCIs'
		END
		INSERT INTO dbo.af_delivery_queue(audience_id,affidavit_id)
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
				ad.audience_id,
				ad.affidavit_id
			FROM
				affidavit_deliveries ad
			WHERE
				ad.media_month_id=@idMediaMonth 
				AND ad.rating_source_id=@idRatingSource;
	
		-- NON-MARRIED ISCI's
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Filling dbo.af_delivery_queue with non-married ISCIs...', 0, 1, @textTimestamp) WITH NOWAIT;
		IF @idAffidavitDeliveryRun IS NOT NULL 
		BEGIN
			WAITFOR DELAY '000:00:00.500'
			INSERT INTO dbo.affidavit_delivery_run_messages SELECT @idAffidavitDeliveryRun,GETDATE(),'Filling dbo.af_delivery_queue with non-married ISCIs'
		END
		INSERT INTO dbo.af_delivery_queue(audience_id,affidavit_id)
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
				ad.audience_id,
				ad.affidavit_id
			FROM
				affidavit_deliveries ad
			WHERE
				ad.media_month_id=@idMediaMonth 
				AND ad.rating_source_id=@idRatingSource;
	
		SET @rows = (SELECT COUNT(1) FROM dbo.af_delivery_queue);
	
		-- update total_remaining_affidavits, time_to_get_remaining_affidavits
		IF @idAffidavitDeliveryRun IS NOT NULL
			UPDATE dbo.affidavit_delivery_runs SET 
				date_last_updated=GETDATE(), 
				total_remaining_affidavits=@rows, 
				time_to_get_remaining_affidavits=DATEDIFF(second, @timestamp, GETDATE()) 
			WHERE 
				id=@idAffidavitDeliveryRun
		
		SET @textRows = LEFT(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1), LEN(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1))-3);
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - %s total rows added to dbo.af_delivery_queue...', 0, 1, @textTimestamp, @textRows) WITH NOWAIT;


		/*******************************************************************************************************************
		STEP 2 OF 5: Get Nielsen Network Rating Map
		*******************************************************************************************************************/
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Filling #temp_nielsen_network_ratings_map...', 0, 1, @textTimestamp) WITH NOWAIT;
		IF @idAffidavitDeliveryRun IS NOT NULL 
		BEGIN
			WAITFOR DELAY '000:00:00.500'
			INSERT INTO dbo.affidavit_delivery_run_messages SELECT @idAffidavitDeliveryRun,getdate(),'Filling #temp_nielsen_network_ratings_map'
		END
	
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
		RAISERROR('%s - %s rows added to #temp_nielsen_network_ratings_map.', 0, 1, @textTimestamp, @textRows) WITH NOWAIT;
	
	
		/*******************************************************************************************************************
		STEP 3 OF 5: Get Rated Nielsen Networks, Days In Month, Networks in Month, and Create a Combined Snapshot
		*******************************************************************************************************************/
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Filling #rated_networks...', 0, 1, @textTimestamp) WITH NOWAIT;
		IF @idAffidavitDeliveryRun IS NOT NULL 
		BEGIN
			WAITFOR DELAY '000:00:00.500'
			INSERT INTO dbo.affidavit_delivery_run_messages SELECT @idAffidavitDeliveryRun,getdate(),'Filling #rated_networks'
		END
		CREATE TABLE #rated_networks (network_id INT, nielsen_network_id INT);		
		INSERT INTO #rated_networks (network_id, nielsen_network_id)
			SELECT
				nm.network_id,
				nn.nielsen_network_id	
			FROM
				rentrak.rentrak.viewership v (NOLOCK)
				JOIN uvw_nielsen_network_maps nnm ON nnm.map_set='Rentrak' 
					AND nnm.map_value=CAST(v.rentrak_id AS VARCHAR)
					AND (nnm.start_date<=@start_date AND (nnm.end_date>=@start_date OR nnm.end_date IS NULL))
				JOIN uvw_nielsen_network_universes nn (NOLOCK) ON nn.nielsen_network_id=nnm.nielsen_network_id
					AND (nn.start_date<=@start_date AND (nn.end_date>=@start_date OR nn.end_date IS NULL))
				JOIN network_maps nm (NOLOCK) ON nm.map_set='Nielsen' 
					AND nm.map_value=CAST(nn.nielsen_id AS VARCHAR)
			WHERE
				v.media_month_id=@idMediaMonth
			GROUP BY
				nm.network_id,
				nn.nielsen_network_id;
	
	
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Filling #days_in_month...', 0, 1, @textTimestamp) WITH NOWAIT;
		IF @idAffidavitDeliveryRun IS NOT NULL 
		BEGIN
			WAITFOR DELAY '000:00:00.500'
			INSERT INTO dbo.affidavit_delivery_run_messages SELECT @idAffidavitDeliveryRun,getdate(),'Filling #days_in_month'
		END
		CREATE TABLE #days_in_month (date_of DATE)
		SET @iterator_date = @start_date
		WHILE @iterator_date <= @end_date
		BEGIN
			INSERT INTO #days_in_month (date_of) VALUES (@iterator_date)
			SET @iterator_date = DATEADD(d,1,@iterator_date)
		END
	
	
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Filling #networks_in_month...', 0, 1, @textTimestamp) WITH NOWAIT
		IF @idAffidavitDeliveryRun IS NOT NULL 
		BEGIN
			WAITFOR DELAY '000:00:00.500'
			INSERT INTO dbo.affidavit_delivery_run_messages SELECT @idAffidavitDeliveryRun,getdate(),'Filling #networks_in_month'
		END
		CREATE TABLE #networks_in_month (network_id INT)
		INSERT INTO #networks_in_month
			SELECT DISTINCT
				a.network_id
			FROM
				dbo.affidavits a
			WHERE
				a.media_month_id=@idMediaMonth
				AND a.status_id=1
			

		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Filling #network_substitution_snapshot...', 0, 1, @textTimestamp) WITH NOWAIT;
		IF @idAffidavitDeliveryRun IS NOT NULL 
		BEGIN
			WAITFOR DELAY '000:00:00.500'
			INSERT INTO dbo.affidavit_delivery_run_messages SELECT @idAffidavitDeliveryRun,getdate(),'Filling #network_substitution_snapshot'
		END
		CREATE TABLE #network_substitution_snapshot (date_of DATE NOT NULL, network_id INT NOT NULL, rating_nielsen_network_id INT NOT NULL, universe_nielsen_network_id INT NOT NULL, rating_weight FLOAT, universe_weight FLOAT);	
		ALTER TABLE #network_substitution_snapshot ADD CONSTRAINT [PK_network_substitution_snapshot] PRIMARY KEY CLUSTERED ([date_of] ASC, [network_id] ASC, [rating_nielsen_network_id] ASC, [universe_nielsen_network_id] ASC)
		INSERT INTO #network_substitution_snapshot	
			SELECT DISTINCT
				d.date_of,
				n.network_id,
				CASE WHEN rn.nielsen_network_id IS NULL THEN nnrm.nielsen_network_id ELSE rn.nielsen_network_id END 'rating_nielsen_network_id',
				CASE WHEN rn.nielsen_network_id IS NULL THEN nnum.nielsen_network_id ELSE rn.nielsen_network_id END 'universe_nielsen_network_id',
				CASE WHEN rn.nielsen_network_id IS NULL THEN nnrm.[weight] ELSE 1.0 END 'rating_weight',
				CASE WHEN rn.nielsen_network_id IS NULL THEN nnum.[weight] ELSE 1.0 END 'universe_weight'
			FROM
				#networks_in_month n
				CROSS APPLY #days_in_month d
				LEFT JOIN #rated_networks rn ON rn.network_id=n.network_id
				JOIN #temp_nielsen_network_ratings_map nnrm ON
					1=nnrm.substitution_category_id
					AND 
					nnrm.network_id=n.network_id
					AND
					d.date_of BETWEEN nnrm.start_date AND nnrm.end_date
					AND
					(nnrm.rating_category_group_id=@rating_category_group_id OR nnrm.rating_category_group_id IS NULL)
				JOIN #temp_nielsen_network_ratings_map nnum ON
					2=nnum.substitution_category_id
					AND
					n.network_id=nnum.network_id
					AND
					d.date_of BETWEEN nnum.start_date AND nnum.end_date
					AND
					(nnum.rating_category_group_id=@rating_category_group_id OR nnum.rating_category_group_id IS NULL)
			UNION
			SELECT DISTINCT
				d.date_of,
				nnrm.network_id,
				nnrm.nielsen_network_id 'rating_nielsen_network_id',
				nnum.nielsen_network_id 'universe_nielsen_network_id',
				1.0 'rating_weight',
				1.0 'universe_weight'
			FROM
				#days_in_month d
				JOIN #temp_nielsen_network_ratings_map nnrm ON
					1=nnrm.substitution_category_id
					AND 
					d.date_of BETWEEN nnrm.start_date AND nnrm.end_date
					AND
					(nnrm.rating_category_group_id=@rating_category_group_id OR nnrm.rating_category_group_id IS NULL)
				JOIN #temp_nielsen_network_ratings_map nnum ON
					2=nnum.substitution_category_id
					AND
					nnum.network_id=nnrm.network_id
					AND
					d.date_of BETWEEN nnum.start_date AND nnum.end_date
					AND
					(nnum.rating_category_group_id=@rating_category_group_id OR nnum.rating_category_group_id IS NULL)
			WHERE
				nnrm.source='daypart_network_map'
	
		/*******************************************************************************************************************
		STEP 4 OF 5: Copy Rentrak Ratings Data to Local DB for Media Month (one demo at a time)
		*******************************************************************************************************************/
		CREATE TABLE #rentrak_networks (rentrak_id INT NOT NULL, nielsen_network_id INT NOT NULL);	
		ALTER TABLE #rentrak_networks ADD CONSTRAINT [PK_delivery_calculation_rentrak_networks] PRIMARY KEY CLUSTERED (rentrak_id ASC, nielsen_network_id ASC)
		INSERT INTO #rentrak_networks
			SELECT
				CAST(nnm.map_value AS INT) 'rentrak_id',
				nnm.nielsen_network_id
			FROM
				uvw_nielsen_network_maps nnm
			WHERE
				nnm.map_set='Rentrak' 
				AND (nnm.start_date<=@start_date AND (nnm.end_date>=@start_date OR nnm.end_date IS NULL))
				
		CREATE TABLE #rentrak_audiences (demographic_number VARCHAR(15) NOT NULL, audience_id INT NOT NULL);	
		ALTER TABLE #rentrak_audiences ADD CONSTRAINT [PK_delivery_calculation_rentrak_audiences] PRIMARY KEY CLUSTERED (demographic_number ASC, audience_id ASC)
		INSERT INTO #rentrak_audiences
			SELECT
				CAST(am.map_value AS INT) 'demographic_number',
				am.audience_id
			FROM
				audience_maps am
			WHERE
				am.map_set='Rentrak'
	
	
		SET @timestamp = GETDATE();
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Copying/Transforming rentrak.viewership Ratings Data for Media Month (one demo at a time to reduce resource consumption)...', 0, 1, @textTimestamp) WITH NOWAIT;
		IF @idAffidavitDeliveryRun IS NOT NULL 
		BEGIN
			WAITFOR DELAY '000:00:00.500'
			INSERT INTO dbo.affidavit_delivery_run_messages SELECT @idAffidavitDeliveryRun,getdate(),'Copying/Transforming rentrak.viewership Ratings Data for Media Month (one demo at a time to reduce resource consumption)'
		END
	
		DECLARE @audience_id INT
		DECLARE AudienceCursor CURSOR FAST_FORWARD FOR
			SELECT DISTINCT ra.audience_id FROM #rentrak_audiences ra
		OPEN AudienceCursor
		FETCH NEXT FROM AudienceCursor INTO @audience_id
		WHILE @@FETCH_STATUS = 0
			BEGIN
				SET @timestamp = GETDATE();
				SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
				RAISERROR('%s - Copying/Transforming rentrak.viewership[audience_id=%d] to dbo.rentrak_viewership...', 0, 1, @textTimestamp, @audience_id) WITH NOWAIT;
				IF @idAffidavitDeliveryRun IS NOT NULL 
				BEGIN
					WAITFOR DELAY '000:00:00.500'
					INSERT INTO dbo.affidavit_delivery_run_messages SELECT @idAffidavitDeliveryRun,getdate(),'Copying/Transforming rentrak.viewership[audience_id=' + CAST(@audience_id AS VARCHAR) + '] to dbo.rentrak_viewership'
				END
			
				INSERT INTO [dbo].[rentrak_viewership]
					SELECT
						v.media_month_id, 25, ra.audience_id, rn.nielsen_network_id, CAST(v.[time] AS DATE) , DATEDIFF(second, CAST(v.[time] AS DATE), v.[time]), DATEDIFF(second, CAST(v.[time] AS DATE), v.[time]) + 899, v.average_audience
					FROM
						rentrak.rentrak.viewership v (NOLOCK)
						JOIN #rentrak_networks rn ON rn.rentrak_id=v.rentrak_id
						JOIN #rentrak_audiences ra ON ra.demographic_number=v.demographic_number
							AND ra.audience_id=@audience_id
					WHERE
						v.media_month_id=@idMediaMonth
					
				FETCH NEXT FROM AudienceCursor INTO @audience_id
			END
		CLOSE AudienceCursor
		DEALLOCATE AudienceCursor
	
		UPDATE STATISTICS rentrak_viewership;
	
		/*******************************************************************************************************************
		STEP 5 OF 5: Calculate Deliveries
		*******************************************************************************************************************/
		SET @rows = 0;
		SET @timestamp = GETDATE(); 
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Filling affidavit_deliveries at %d intervals...', 0, 1, @textTimestamp, @batchSize) WITH NOWAIT;
		IF @idAffidavitDeliveryRun IS NOT NULL 
		BEGIN
			WAITFOR DELAY '000:00:00.500'
			INSERT INTO dbo.affidavit_delivery_run_messages SELECT @idAffidavitDeliveryRun,getdate(),'Filling affidavit_deliveries at ' + CAST(@batchSize AS VARCHAR) + ' intervals'
		END
	
		CREATE TABLE #batch(audience_id INT NOT NULL, affidavit_id BIGINT NOT NULL);
		ALTER TABLE #batch ADD CONSTRAINT [PK_temp_batch_affidavit_processing] PRIMARY KEY CLUSTERED ([audience_id] ASC, [affidavit_id] ASC)
		CREATE TABLE #expanded_batch (rating_category_id TINYINT NOT NULL, custom_audience_id INT NOT NULL, rating_audience_id INT NOT NULL, rating_nielsen_network_id INT NOT NULL, universe_nielsen_network_id INT NOT NULL, est_air_date DATE NOT NULL, est_air_time INT NOT NULL, affidavit_id BIGINT NOT NULL, rating_weight FLOAT NOT NULL, universe_weight FLOAT NOT NULL)
		ALTER TABLE #expanded_batch ADD CONSTRAINT [PK_temp_expanded_batch_affidavit_processing] PRIMARY KEY CLUSTERED (rating_category_id ASC, custom_audience_id ASC, rating_audience_id ASC, rating_nielsen_network_id ASC, universe_nielsen_network_id ASC, est_air_date ASC, est_air_time ASC, affidavit_id ASC)

		WHILE 1 = 1
		BEGIN
			IF @stopHour IS NOT NULL
			BEGIN
				SELECT @currentHour = CAST(CASE WHEN LEN(CAST(DATEPART(hh,GETDATE()) AS VARCHAR)) = 1 THEN '0' + CAST(DATEPART(hh,GETDATE()) AS VARCHAR) ELSE CAST(DATEPART(hh,GETDATE()) AS VARCHAR) END + CASE WHEN LEN(CAST(DATEPART(mi,GETDATE()) AS VARCHAR)) = 1 THEN '0' + CAST(DATEPART(mi,GETDATE()) AS VARCHAR) ELSE CAST(DATEPART(mi,GETDATE()) AS VARCHAR) END AS INT)
				IF @currentHour >= @stopHour
				BEGIN
					SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
					RAISERROR('%s - Past stop hour, ending run...', 0, 1, @textTimestamp, @batchSize) WITH NOWAIT;
					IF @idAffidavitDeliveryRun IS NOT NULL 
					BEGIN
						WAITFOR DELAY '000:00:00.500'
						INSERT INTO dbo.affidavit_delivery_run_messages SELECT @idAffidavitDeliveryRun,getdate(),'Past stop hour, ending run'
					END
					BREAK;
				END
			END

			INSERT INTO #batch (audience_id,affidavit_id)
				SELECT
					TOP(@batchSize) d.audience_id,d.affidavit_id
				FROM
					dbo.af_delivery_queue d;
		
			BEGIN TRY								
				BEGIN TRANSACTION T1;
			
				SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
				RAISERROR('%s - Preparing Insert (expanding batch)...', 0, 1, @textTimestamp) WITH NOWAIT;
				IF @idAffidavitDeliveryRun IS NOT NULL 
				BEGIN
					WAITFOR DELAY '000:00:00.500'
					INSERT INTO dbo.affidavit_delivery_run_messages SELECT @idAffidavitDeliveryRun,getdate(),'Preparing Insert (expanding batch)'
				END
				INSERT INTO #expanded_batch
					SELECT
						rsrc.rating_category_id,
						aa.custom_audience_id,
						aa.rating_audience_id,
						nss.rating_nielsen_network_id,
						nss.universe_nielsen_network_id,
						ISNULL(a.adjusted_air_date, a.air_date),
						ISNULL(a.adjusted_air_time, a.air_time),
						rap.affidavit_id,
						nss.rating_weight,
						nss.universe_weight
					FROM
						#batch rap
						JOIN affidavits a (NOLOCK) ON a.media_month_id=@idMediaMonth 
							AND a.id=rap.affidavit_id
						JOIN #network_substitution_snapshot nss ON a.air_date=nss.date_of
							AND nss.network_id=a.network_id
						JOIN dbo.audience_audiences aa (NOLOCK) ON aa.custom_audience_id=rap.audience_id
							AND aa.rating_category_group_id=@rating_category_group_id
						JOIN dbo.rating_source_rating_categories rsrc (NOLOCK) ON rsrc.rating_source_id=@idRatingSource;
			
				SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
				RAISERROR('%s - Inserting Affidavit Deliveries...', 0, 1, @textTimestamp) WITH NOWAIT;
				IF @idAffidavitDeliveryRun IS NOT NULL 
				BEGIN
					WAITFOR DELAY '000:00:00.500'
					INSERT INTO dbo.affidavit_delivery_run_messages SELECT @idAffidavitDeliveryRun,getdate(),'Inserting affidavit_deliveries'
				END
				INSERT INTO dbo.affidavit_deliveries(media_month_id, rating_source_id, audience_id, affidavit_id, audience_usage, universe, regional_usage, regional_rating)
					SELECT
						@idMediaMonth,
						@idRatingSource,
						rap.custom_audience_id,
						rap.affidavit_id,
						SUM(v.usage) * rap.rating_weight 'national_audience_usage',		-- note the SUM here is for combining custom audiences
						SUM(u.universe) * rap.universe_weight 'national_universe',		-- likewise
						NULL 'regional_usage',
						NULL 'regional_rating'
					FROM
						#expanded_batch rap						
						JOIN rentrak_viewership v (NOLOCK) ON v.media_month_id=@idMediaMonth
							AND v.rating_category_id=rap.rating_category_id
							AND v.audience_id=rap.rating_audience_id
							AND v.nielsen_network_id=rap.rating_nielsen_network_id
							AND v.rating_date = rap.est_air_date
							AND rap.est_air_time BETWEEN v.start_time AND v.end_time
						JOIN universes u (NOLOCK) ON u.rating_category_id=rap.rating_category_id
							AND u.forecast_media_month_id=@idMediaMonth
							AND u.audience_id=rap.rating_audience_id
							AND u.nielsen_network_id=rap.universe_nielsen_network_id
					GROUP BY
						rap.affidavit_id, 
						rap.custom_audience_id,
						rap.rating_weight,
						rap.universe_weight;
									
				SET @subrowcount = @@ROWCOUNT
				SET @rows = @rows + @subrowcount;
				SET @textRows = LEFT(CONVERT(VARCHAR, CAST(@subrowcount AS MONEY), 1), LEN(CONVERT(VARCHAR, CAST(@subrowcount AS MONEY), 1))-3);
				SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
				SET @remainingrowcount = (SELECT COUNT(*) FROM dbo.af_delivery_queue) - @batchSize
				IF @remainingrowcount < 0
					SET @remainingrowcount = 0
				RAISERROR('%s - %s total rows added to affidavit_deliveries with %d remaining...', 0, 1, @textTimestamp, @textRows, @remainingrowcount) WITH NOWAIT;
						
				COMMIT TRANSACTION T1;
			
				IF @idAffidavitDeliveryRun IS NOT NULL
					UPDATE dbo.affidavit_delivery_runs SET date_last_updated=GETDATE(), num_processed=@rows, num_remaining=@remainingrowcount WHERE id=@idAffidavitDeliveryRun
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
				dbo.af_delivery_queue
			FROM
				dbo.af_delivery_queue d
				JOIN #batch b ON b.audience_id=d.audience_id
					AND b.affidavit_id=d.affidavit_id 
			
			-- clear processing tables for next batch
			TRUNCATE TABLE #batch;
			TRUNCATE TABLE #expanded_batch;

			IF @remainingrowcount <= 0 BREAK
		END
	
		SET @textRows = LEFT(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1), LEN(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1))-3);
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Estimating delivery for %s is done. %s rows added to table.', 0, 1, @textTimestamp, @codeMediaMonth, @textRows) WITH NOWAIT;
		IF @idAffidavitDeliveryRun IS NOT NULL
			UPDATE dbo.affidavit_delivery_runs SET date_last_updated=GETDATE(), date_completed=GETDATE(), time_to_calculate_deliveries=DATEDIFF(second, @timestamp, GETDATE()) WHERE id=@idAffidavitDeliveryRun
		IF @idAffidavitDeliveryRun IS NOT NULL 
		BEGIN
			WAITFOR DELAY '000:00:00.500'
			INSERT INTO dbo.affidavit_delivery_run_messages SELECT @idAffidavitDeliveryRun,getdate(),'Processing completed successfully'
		END
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

	TRUNCATE TABLE [dbo].[rentrak_viewership];
	IF OBJECT_ID('.af_delivery_queue') IS NOT NULL DROP TABLE dbo.af_delivery_queue;
	IF OBJECT_ID('.rentrak_universes') IS NOT NULL DROP TABLE dbo.rentrak_universes;
	IF OBJECT_ID('tempdb..#temp_nielsen_network_ratings_map') IS NOT NULL DROP TABLE #temp_nielsen_network_ratings_map;
	IF OBJECT_ID('tempdb..#batch') IS NOT NULL DROP TABLE #batch;
	IF OBJECT_ID('tempdb..#rated_networks') IS NOT NULL DROP TABLE #rated_networks;
	IF OBJECT_ID('tempdb..#networks_in_month') IS NOT NULL DROP TABLE #networks_in_month;
	IF OBJECT_ID('tempdb..#days_in_month') IS NOT NULL DROP TABLE #days_in_month;
	IF OBJECT_ID('tempdb..#rated_networks') IS NOT NULL DROP TABLE #rated_networks;

	SET NOCOUNT OFF;
END
