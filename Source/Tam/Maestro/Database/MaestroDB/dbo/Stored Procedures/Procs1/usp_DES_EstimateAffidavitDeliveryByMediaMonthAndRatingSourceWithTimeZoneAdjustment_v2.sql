-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/20/2012
-- Modified:	06/27/2016 - Added ability to take in and filter on proposal Ids
--				01/22/2015 - 8315: modified logic which determines affidavit/audience combinations to calculate deliveyr for
--				05/15/2015 - Updated to support NULL adjusted_air_date and adjusted_air_time
--				12/16/2015 - Modified to use temp_db_backup as a staging area and removed EXCEPT clause causing excess CPU usage in the DB server.
--				04/19/2016 - Modified to use temp_db_backup for the current batch (similar to delivery calculations in post logs).
--				06/13/2016 - Modfiied to prevent parameter sniffing.
--				06/25/2016 - Modified to add @proposalIds parameter.
-- Description:	Calculates delivery for affidavits.
--				This takes into account time zone shifts by zone and has special logic to check if a zone has become rated
--				in which case it will cease using a network substitution.
--				Note @idRatingSource is translated into rating categories.
-- =============================================
-- usp_DES_EstimateAffidavitDeliveryByMediaMonthAndRatingSourceWithTimeZoneAdjustment_v2 NULL,381,1,NULL
CREATE PROCEDURE [dbo].[usp_DES_EstimateAffidavitDeliveryByMediaMonthAndRatingSourceWithTimeZoneAdjustment_v2]
	@idAffidavitDeliveryRun INT,
	@idMediaMonth INT,
	@idRatingSource INT,
	@stopHour INT,
	@proposalIds NVARCHAR(MAX)
WITH RECOMPILE
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

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
			@nielsen_nad_start_date DATETIME,
			@nielsen_nad_end_date DATETIME,
			@subrowcount INT,
			@remainingrowcount INT,
			@batchSize INT,
			@monthYear INT,
			@currentHour INT,
			@rating_category_group_id TINYINT,
			@affidavit_delivery_run_id INT,
            @media_month_id INT,
            @rating_source_id INT,
            @stop_hour INT;

		-- eliminate potentional parameter sniffing issue by repeating input variables
        SET @affidavit_delivery_run_id = @idAffidavitDeliveryRun;
        SET @media_month_id = @idMediaMonth;
        SET @rating_source_id = @idRatingSource;
        SET @stop_hour = @stopHour;
				
		-- determine nielsen_nad quarter to use, either it's already been used for this month and stored in nielsen_nad_applications or it 
		--	 hasn't in which case we'll add it, we want to keep the nielsen nad quarter used consistent across delivery calculations.
		IF (SELECT COUNT(1) FROM dbo.nielsen_nad_applications WHERE media_month_id=@media_month_id) = 0
			INSERT INTO dbo.nielsen_nad_applications (media_month_id,start_date,end_date)
				SELECT TOP 1 @media_month_id,start_date,end_date FROM dbo.uvw_nielsen_nad ORDER BY start_date DESC
	
		-- get current nielsen nad start/end date
		SELECT @nielsen_nad_start_date = nna.start_date, @nielsen_nad_end_date = nna.end_date FROM dbo.nielsen_nad_applications nna WHERE  nna.media_month_id=@media_month_id

		SELECT @start_date=mm.start_date, @end_date=mm.end_date FROM dbo.media_months mm WHERE mm.id=@media_month_id
		SET @batchSize = 500000;

		IF @affidavit_delivery_run_id  IS NOT NULL
			UPDATE dbo.affidavit_delivery_runs SET date_started=GETDATE(),date_last_updated=GETDATE() WHERE id=@affidavit_delivery_run_id
		
		SELECT @codeMediaMonth = mm.media_month, @monthYear = mm.[year] FROM dbo.media_months mm WHERE mm.id = @media_month_id
		SELECT @codeRatingSource = rs.code FROM rating_sources rs WHERE rs.id = @rating_source_id
		SELECT @rating_category_group_id = dbo.GetRatingCategoryGroupIdOfRatingsSource(@rating_source_id)
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		SET @idHHAudience = dbo.GetIDFromAudienceString('hh');

		RAISERROR('%s - Estimating %s delivery for %s...', 0, 1, @textTimestamp, @codeRatingSource, @codeMediaMonth) WITH NOWAIT;

		-- update total_done_affidavits
		IF @affidavit_delivery_run_id IS NOT NULL
		BEGIN
			SELECT @rows = COUNT(1) FROM affidavit_deliveries ad WHERE ad.media_month_id=@media_month_id AND ad.rating_source_id=@rating_source_id;

			UPDATE dbo.affidavit_delivery_runs SET 
				date_last_updated=GETDATE(), 
				total_done_affidavits=@rows, 
				time_to_get_done_affidavits=0 
			WHERE 
				id=@affidavit_delivery_run_id;
		END
	
		IF OBJECT_ID('temp_db_backup.dbo.affidavit_delivery_queue') IS NOT NULL DROP TABLE temp_db_backup.dbo.affidavit_delivery_queue;
		CREATE TABLE temp_db_backup.dbo.affidavit_delivery_queue (audience_id INT NOT NULL, affidavit_id BIGINT NOT NULL, PRIMARY KEY CLUSTERED(audience_id ASC, affidavit_id ASC));
 
		SET @timestamp = GETDATE();
 	
 		/*******************************************************************************************************************
		STEP 1 OF 4: Queue Affidavits to Calculate Delivery For
		*******************************************************************************************************************/
		-- MARRIED ISCI's
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Filling temp_db_backup.dbo.affidavit_delivery_queue with married ISCIs...', 0, 1, @textTimestamp) WITH NOWAIT;
		IF @affidavit_delivery_run_id IS NOT NULL 
		BEGIN
			WAITFOR DELAY '000:00:00.500'
			INSERT INTO dbo.affidavit_delivery_run_messages SELECT @affidavit_delivery_run_id,GETDATE(),'Filling temp_db_backup.dbo.affidavit_delivery_queue with married ISCIs'
		END
		INSERT INTO temp_db_backup.dbo.affidavit_delivery_queue(audience_id,affidavit_id)
			SELECT DISTINCT
				pa.audience_id,
				a.id 'affidavit_id'
			FROM
				proposals p
				JOIN proposal_materials pm	 ON pm.proposal_id=p.id
				JOIN material_revisions mr	 ON mr.revised_material_id=pm.material_id
				JOIN proposal_audiences pa	 ON pa.proposal_id IN (p.id, p.original_proposal_id)
				JOIN affidavits a			 ON a.media_month_id=@media_month_id AND a.material_id=mr.original_material_id
			WHERE
				p.posting_media_month_id=@media_month_id
				AND a.status_id=1 -- VALID
				AND (
					-- we want to calculate MIT Live delivery for all posting plans regardless of inputted ratings source
					@rating_source_id=1
					-- the posting plan has the rating source being run
					OR p.rating_source_id=@rating_source_id
					-- all posting plans in a post which has the rating source being run
					OR p.id IN (
						SELECT DISTINCT
							p2.id 
						FROM 
							tam_post_proposals tpp
							JOIN tam_posts tp ON tp.id=tpp.tam_post_id
								AND tp.rating_source_id=@rating_source_id
							JOIN proposals p2 ON p2.id=tpp.posting_plan_proposal_id
								AND p2.posting_media_month_id=@media_month_id
					)
				)
				AND (@proposalIds is null or p.id in (select id from SplitIntegers(@proposalIds)))
	
		-- NON-MARRIED ISCI's
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Filling temp_db_backup.dbo.affidavit_delivery_queue with non-married ISCIs...', 0, 1, @textTimestamp) WITH NOWAIT;
		IF @affidavit_delivery_run_id IS NOT NULL 
		BEGIN
			WAITFOR DELAY '000:00:00.500'
			INSERT INTO dbo.affidavit_delivery_run_messages SELECT @affidavit_delivery_run_id,GETDATE(),'Filling temp_db_backup.dbo.affidavit_delivery_queue with non-married ISCIs'
		END
		INSERT INTO temp_db_backup.dbo.affidavit_delivery_queue(audience_id,affidavit_id)
			SELECT DISTINCT
				pa.audience_id,
				a.id 'affidavit_id'
			FROM
				proposals p
				JOIN proposal_materials pm	 ON pm.proposal_id=p.id
				JOIN proposal_audiences pa	 ON pa.proposal_id IN (p.id, p.original_proposal_id)
				JOIN affidavits a			 ON a.media_month_id=@media_month_id AND a.material_id=pm.material_id
			WHERE
				p.posting_media_month_id=@media_month_id
				AND a.status_id=1 -- VALID
				AND (
					-- we want to calculate MIT Live delivery for all posting plans regardless of inputted ratings source
					@rating_source_id=1
					-- the posting plan has the rating source being run
					OR p.rating_source_id=@rating_source_id
					-- all posting plans in a post which has the rating source being run
					OR p.id IN (
						SELECT DISTINCT
							p2.id 
						FROM 
							tam_post_proposals tpp
							JOIN tam_posts tp ON tp.id=tpp.tam_post_id
								AND tp.rating_source_id=@rating_source_id
							JOIN proposals p2 ON p2.id=tpp.posting_plan_proposal_id
								AND p2.posting_media_month_id=@media_month_id
					)
				)
				AND (@proposalIds is null or p.id in (select id from SplitIntegers(@proposalIds)))

		-- remove already calculated combinations
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Removing afidavit/audience pairs from temp_db_backup.dbo.new_delivery_calculation_queue that have already been processed ...', 0, 1, @textTimestamp) WITH NOWAIT;
		DELETE
			temp_db_backup.dbo.affidavit_delivery_queue
		FROM
			temp_db_backup.dbo.affidavit_delivery_queue dcq
			JOIN affidavit_deliveries ad ON
				ad.audience_id = dcq.audience_id
				AND ad.affidavit_id = dcq.affidavit_id
		WHERE
			ad.media_month_id=@media_month_id 
			AND ad.rating_source_id=@rating_source_id;
	
		SET @rows = (SELECT COUNT(1) FROM temp_db_backup.dbo.affidavit_delivery_queue);
	
		-- update total_remaining_affidavits, time_to_get_remaining_affidavits
		IF @affidavit_delivery_run_id IS NOT NULL
			UPDATE dbo.affidavit_delivery_runs SET 
				date_last_updated=GETDATE(), 
				total_remaining_affidavits=@rows, 
				time_to_get_remaining_affidavits=DATEDIFF(second, @timestamp, GETDATE()) 
			WHERE 
				id=@affidavit_delivery_run_id;
		
		SET @textRows = LEFT(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1), LEN(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1))-3);
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - %s total rows added to temp_db_backup.dbo.affidavit_delivery_queue...', 0, 1, @textTimestamp, @textRows) WITH NOWAIT;


		/*******************************************************************************************************************
		STEP 2 OF 4: Get Nielsen Network Rating Map
		*******************************************************************************************************************/
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Filling #nielsen_network_ratings_map...', 0, 1, @textTimestamp) WITH NOWAIT;
		IF @affidavit_delivery_run_id IS NOT NULL 
		BEGIN
			WAITFOR DELAY '000:00:00.500'
			INSERT INTO dbo.affidavit_delivery_run_messages SELECT @affidavit_delivery_run_id,getdate(),'Filling #nielsen_network_ratings_map'
		END
	
		CREATE TABLE #nielsen_network_ratings_map (date_of DATETIME NOT NULL, network_id INT NOT NULL, nielsen_network_id INT NULL, rating_weight FLOAT NOT NULL, universe_weight FLOAT NOT NULL, rating_network_id INT NULL, universe_network_id INT NULL, rating_nielsen_network_id INT NULL, universe_nielsen_network_id INT NULL, PRIMARY KEY CLUSTERED(date_of ASC, network_id ASC))
		INSERT INTO #nielsen_network_ratings_map(date_of,network_id,nielsen_network_id,rating_weight,universe_weight,rating_network_id,universe_network_id,rating_nielsen_network_id,universe_nielsen_network_id)
			EXEC usp_PST_GetMonthlyPostingNetworkToNielsenNetworkAndSubstitutionMap @media_month_id, @rating_source_id
	
		SET @rows = @@ROWCOUNT;
		SET @textRows = LEFT(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1), LEN(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1))-3);	
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - %s rows added to #nielsen_network_ratings_map.', 0, 1, @textTimestamp, @textRows) WITH NOWAIT;
	
	
		/*******************************************************************************************************************
		STEP 3 OF 4: Get Nielsen Regional Ratings Data
		*******************************************************************************************************************/
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Filling #market_breaks_counties...', 0, 1, @textTimestamp) WITH NOWAIT;
		IF @affidavit_delivery_run_id IS NOT NULL 
		BEGIN
			WAITFOR DELAY '000:00:00.500'
			INSERT INTO dbo.affidavit_delivery_run_messages SELECT @affidavit_delivery_run_id,getdate(),'Filling #market_breaks_counties'
		END
		CREATE TABLE #market_breaks_counties (market_break VARCHAR(63) NOT NULL, dma_id INT NOT NULL, dma_name VARCHAR(63) NOT NULL, total_counties INT NOT NULL, PRIMARY KEY CLUSTERED(market_break ASC, dma_id ASC, dma_name ASC))
		INSERT INTO #market_breaks_counties
			SELECT
				nmbc.market_break,
				ncue.dma_id,
				ncue.dma_name,
				COUNT(1) 'counties'
			FROM
				dbo.nielsen_market_break_counties nmbc
				JOIN dbo.nielsen_county_universe_estimates ncue ON ncue.nmr_county_code=nmbc.nmr_county_code
			GROUP BY
				nmbc.market_break,
				ncue.dma_id,
				ncue.dma_name
			ORDER BY
				nmbc.market_break,
				ncue.dma_id,
				ncue.dma_name
	
	
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Filling #market_breaks...', 0, 1, @textTimestamp) WITH NOWAIT;
		IF @affidavit_delivery_run_id IS NOT NULL 
		BEGIN
			WAITFOR DELAY '000:00:00.500'
			INSERT INTO dbo.affidavit_delivery_run_messages SELECT @affidavit_delivery_run_id,getdate(),'Filling #market_breaks'
		END
		CREATE TABLE #market_breaks (dma_id INT NOT NULL, market_break VARCHAR(63) NOT NULL, dma_name VARCHAR(63) NOT NULL, total_counties INT, PRIMARY KEY CLUSTERED (dma_id ASC, market_break ASC))
		INSERT INTO #market_breaks
			SELECT
				mbc.dma_id,
				mbc.market_break,
				mbc.dma_name,
				mbc.total_counties
			FROM
			(
				SELECT
					dma_id,
					dma_name,
					MAX(total_counties) 'max_counties'
				FROM
					#market_breaks_counties
				GROUP BY
					dma_id,
					dma_name
			) tmp
			JOIN #market_breaks_counties mbc ON mbc.dma_id=tmp.dma_id
				AND mbc.total_counties=tmp.max_counties
		
		
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Filling #nielsen_nad...', 0, 1, @textTimestamp) WITH NOWAIT;
		IF @affidavit_delivery_run_id IS NOT NULL 
		BEGIN
			WAITFOR DELAY '000:00:00.500'
			INSERT INTO dbo.affidavit_delivery_run_messages SELECT @affidavit_delivery_run_id,getdate(),'Filling #nielsen_nad'
		END
		CREATE TABLE #nielsen_nad (market_break VARCHAR(63) NOT NULL, network_id INT NOT NULL, audience_id INT NOT NULL, universe INT, rating FLOAT, delivery FLOAT, composite_universe INT, composite_rating FLOAT, composite_delivery FLOAT, start_time INT NOT NULL, end_time INT NOT NULL, mon BIT NOT NULL,tue BIT NOT NULL, wed BIT NOT NULL, thu BIT NOT NULL, fri BIT NOT NULL, sat BIT NOT NULL, sun BIT NOT NULL, PRIMARY KEY CLUSTERED (market_break ASC,network_id ASC,audience_id ASC,start_time ASC,end_time ASC,mon ASC,tue ASC,wed ASC,thu ASC,fri ASC,sat ASC,sun ASC))
		INSERT INTO #nielsen_nad
			SELECT
				nad.market_break,
				nad.network_id,
				nad.audience_id,
				nad.universe,
				nad.rating,
				nad.delivery,
				nad.composite_universe,
				nad.composite_rating,
				nad.composite_delivery,
				nad.start_time,
				nad.end_time,
				nad.mon,
				nad.tue,
				nad.wed,
				nad.thu,
				nad.fri,
				nad.sat,
				nad.sun
			FROM
				dbo.uvw_nielsen_nad nad
			WHERE
				nad.start_date=@nielsen_nad_start_date
				AND nad.end_date=@nielsen_nad_end_date
			ORDER BY
				nad.market_break,
				nad.network_id,
				nad.audience_id,
				nad.universe,
				nad.rating,
				nad.delivery,
				nad.composite_universe,
				nad.composite_rating,
				nad.composite_delivery,
				nad.start_time,
				nad.end_time,
				nad.mon,
				nad.tue,
				nad.wed,
				nad.thu,
				nad.fri,
				nad.sat,
				nad.sun;
	
		IF OBJECT_ID('tempdb..#market_breaks_counties') > 0 
			DROP TABLE #market_breaks_counties;

		/*******************************************************************************************************************
		STEP 4 OF 4: Calculate Deliveries
		*******************************************************************************************************************/
		SET @rows = 0;
		SET @timestamp = GETDATE();
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Filling affidavit_deliveries at %d intervals...', 0, 1, @textTimestamp, @batchSize) WITH NOWAIT;
		IF @affidavit_delivery_run_id IS NOT NULL 
		BEGIN
			WAITFOR DELAY '000:00:00.500'
			INSERT INTO dbo.affidavit_delivery_run_messages SELECT @affidavit_delivery_run_id,getdate(),'Filling affidavit_deliveries at ' + CAST(@batchSize AS VARCHAR) + ' intervals'
		END
	
		IF OBJECT_ID('temp_db_backup.dbo.affidavit_delivery_calculation_batch') IS NOT NULL DROP TABLE temp_db_backup.dbo.affidavit_delivery_calculation_batch;		
		CREATE TABLE temp_db_backup.dbo.affidavit_delivery_calculation_batch (audience_id INT NOT NULL, affidavit_id BIGINT NOT NULL, air_date DATETIME NOT NULL, air_time INT NOT NULL, media_month_id_from_air_date INT NOT NULL, network_id INT NOT NULL, dma_id INT NOT NULL, PRIMARY KEY CLUSTERED (audience_id ASC, affidavit_id ASC));

		WHILE 1 = 1
		BEGIN
			IF @stop_hour IS NOT NULL
			BEGIN
				SELECT @currentHour = CAST(CASE WHEN LEN(CAST(DATEPART(hh,GETDATE()) AS VARCHAR)) = 1 THEN '0' + CAST(DATEPART(hh,GETDATE()) AS VARCHAR) ELSE CAST(DATEPART(hh,GETDATE()) AS VARCHAR) END + CASE WHEN LEN(CAST(DATEPART(mi,GETDATE()) AS VARCHAR)) = 1 THEN '0' + CAST(DATEPART(mi,GETDATE()) AS VARCHAR) ELSE CAST(DATEPART(mi,GETDATE()) AS VARCHAR) END AS INT)
				IF @currentHour >= @stop_hour
				BEGIN
					SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
					RAISERROR('%s - Past stop hour, ending run...', 0, 1, @textTimestamp, @batchSize) WITH NOWAIT;
					IF @affidavit_delivery_run_id IS NOT NULL 
					BEGIN
						WAITFOR DELAY '000:00:00.500'
						INSERT INTO dbo.affidavit_delivery_run_messages SELECT @affidavit_delivery_run_id,getdate(),'Past stop hour, ending run'
					END
					BREAK;
				END
			END
		
			INSERT INTO temp_db_backup.dbo.affidavit_delivery_calculation_batch (audience_id,affidavit_id,air_date,air_time,media_month_id_from_air_date,network_id,dma_id)
				SELECT
					TOP(@batchSize) d.audience_id,d.affidavit_id,ISNULL(a.adjusted_air_date, a.air_date),ISNULL(a.adjusted_air_time, a.air_time),mm.id,a.network_id,zd.dma_id
				FROM
					temp_db_backup.dbo.affidavit_delivery_queue d
					JOIN affidavits a ON a.media_month_id=@media_month_id 
						AND a.id=d.affidavit_id
					JOIN dbo.uvw_zonedma_universe zd ON zd.zone_id=a.zone_id 
						AND zd.start_date<=a.air_date AND (zd.end_date>=a.air_date OR zd.end_date IS NULL)
					JOIN media_months mm (NOLOCK) ON ISNULL(a.adjusted_air_date, a.air_date) BETWEEN mm.start_date AND mm.end_date
				ORDER BY
					d.audience_id,d.affidavit_id
		
			BEGIN TRY
				SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
				RAISERROR('%s - Getting Regional Factors...', 0, 1, @textTimestamp) WITH NOWAIT;
				IF @affidavit_delivery_run_id IS NOT NULL 
				BEGIN
					WAITFOR DELAY '000:00:00.500'
					INSERT INTO dbo.affidavit_delivery_run_messages SELECT @affidavit_delivery_run_id,getdate(),'Calculating Regional Factors'
				END
			
				IF OBJECT_ID('temp_db_backup.dbo.affidavit_delivery_calculation_regional') IS NOT NULL DROP TABLE temp_db_backup.dbo.affidavit_delivery_calculation_regional;		
				CREATE TABLE temp_db_backup.dbo.affidavit_delivery_calculation_regional(audience_id INT NOT NULL, affidavit_id BIGINT NOT NULL, regional_rating FLOAT, regional_factor FLOAT, PRIMARY KEY CLUSTERED (audience_id ASC,affidavit_id ASC));
				INSERT INTO temp_db_backup.dbo.affidavit_delivery_calculation_regional (audience_id,affidavit_id,regional_rating,regional_factor)
					SELECT
						adcb.audience_id,
						adcb.affidavit_id,
						SUM(nad.delivery) / NULLIF(SUM(nad.universe),0) 'regional_rating',
						(SUM(nad.delivery) / NULLIF(SUM(nad.universe),0)) / NULLIF(SUM(nad.composite_delivery) / NULLIF(SUM(nad.composite_universe),0),0) 'regional_factor'
					FROM
						temp_db_backup.dbo.affidavit_delivery_calculation_batch adcb
						JOIN dbo.audience_audiences aa ON aa.custom_audience_id=adcb.audience_id
							AND aa.rating_category_group_id=@rating_category_group_id
						JOIN dbo.audiences ar ON ar.id=aa.rating_audience_id
						JOIN #market_breaks mb ON mb.dma_id=adcb.dma_id
						JOIN #nielsen_nad nad ON nad.network_id=adcb.network_id 
							AND nad.market_break=mb.market_break
							AND 1=(
								CASE WHEN nad.end_time < nad.start_time THEN 
									CASE WHEN adcb.air_time BETWEEN nad.start_time AND 86400 OR adcb.air_time BETWEEN 0 AND nad.end_time THEN 1 ELSE 0 END
								ELSE
									CASE WHEN adcb.air_time BETWEEN nad.start_time AND nad.end_time THEN 1 ELSE 0 END
								END
							)
							AND 1=(
								CASE DATEPART(weekday,adcb.air_date) WHEN 1 THEN nad.sun WHEN 2 THEN nad.mon WHEN 3 THEN nad.tue WHEN 4 THEN nad.wed WHEN 5 THEN nad.thu WHEN 6 THEN nad.fri WHEN 7 THEN nad.sat END
							)
						JOIN dbo.audiences a_nad ON a_nad.id=nad.audience_id
					WHERE
						((aa.rating_audience_id=31 AND a_nad.id=31) OR (aa.rating_audience_id<>31 AND a_nad.id<>31 AND (a_nad.range_start <= ar.range_end AND a_nad.range_end >= ar.range_start)))
					GROUP BY
						adcb.audience_id,
						adcb.affidavit_id
							
				SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
				RAISERROR('%s - Inserting Affidavit Deliveries...', 0, 1, @textTimestamp) WITH NOWAIT;
				IF @affidavit_delivery_run_id IS NOT NULL 
				BEGIN
					WAITFOR DELAY '000:00:00.500'
					INSERT INTO dbo.affidavit_delivery_run_messages SELECT @affidavit_delivery_run_id,getdate(),'Inserting affidavit_deliveries'
				END
				BEGIN TRANSACTION T1;
			
				INSERT INTO dbo.affidavit_deliveries (media_month_id, rating_source_id, audience_id, affidavit_id, audience_usage, universe, regional_usage, regional_rating)
					SELECT
						@media_month_id,
						@rating_source_id,
						rap.audience_id,
						rap.affidavit_id,
						SUM(mpa.usage) * nnrm.rating_weight 'national_audience_usage',	-- note the SUM here is for combining custom audiences
						SUM(mua.universe) * nnrm.universe_weight 'national_universe',		-- likewise
						(SUM(mpa.usage) * nnrm.rating_weight) * regional.regional_factor 'regional_usage',
						regional.regional_rating
					FROM
						temp_db_backup.dbo.affidavit_delivery_calculation_batch rap
						JOIN #nielsen_network_ratings_map nnrm ON nnrm.network_id=rap.network_id
							AND nnrm.date_of=rap.air_date
						JOIN dbo.audience_audiences aa ON aa.custom_audience_id=rap.audience_id
							AND aa.rating_category_group_id=@rating_category_group_id
						JOIN dbo.rating_source_rating_categories rsrc ON rsrc.rating_source_id=@rating_source_id
						JOIN dbo.mit_ratings mr ON mr.media_month_id=rap.media_month_id_from_air_date
							AND mr.rating_category_id=rsrc.rating_category_id
							AND mr.nielsen_network_id=nnrm.rating_nielsen_network_id
							AND mr.rating_date=rap.air_date
							AND rap.air_time BETWEEN mr.start_time AND mr.end_time
						JOIN dbo.mit_person_audiences mpa ON mpa.media_month_id=rap.media_month_id_from_air_date
							AND mpa.mit_rating_id=mr.id
							AND mpa.audience_id=aa.rating_audience_id
						JOIN dbo.mit_universes mu ON mu.media_month_id=@media_month_id
							AND mu.rating_category_id=rsrc.rating_category_id
							AND mu.nielsen_network_id=nnrm.universe_nielsen_network_id
							AND rap.air_date BETWEEN mu.[start_date] AND mu.end_date
						JOIN dbo.mit_universe_audiences mua ON mua.media_month_id=@media_month_id
							AND mua.mit_universe_id=mu.id
							AND mua.audience_id=aa.rating_audience_id
						LEFT JOIN temp_db_backup.dbo.affidavit_delivery_calculation_regional regional ON regional.audience_id=rap.audience_id
							AND regional.affidavit_id=rap.affidavit_id
					GROUP BY
						rap.affidavit_id, 
						rap.audience_id,
						nnrm.rating_weight,
						nnrm.universe_weight,
						regional.regional_rating,
						regional.regional_factor;
									
				SET @subrowcount = @@ROWCOUNT
				SET @rows = @rows + @subrowcount;
				SET @textRows = LEFT(CONVERT(VARCHAR, CAST(@subrowcount AS MONEY), 1), LEN(CONVERT(VARCHAR, CAST(@subrowcount AS MONEY), 1))-3);
				SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
				SET @remainingrowcount = (SELECT COUNT(1) FROM temp_db_backup.dbo.affidavit_delivery_queue) - @batchSize
				IF @remainingrowcount < 0
					SET @remainingrowcount = 0
				RAISERROR('%s - %s total rows added to affidavit_deliveries with %d remaining...', 0, 1, @textTimestamp, @textRows, @remainingrowcount) WITH NOWAIT;
						
				COMMIT TRANSACTION T1;
			
				IF @affidavit_delivery_run_id IS NOT NULL
					UPDATE dbo.affidavit_delivery_runs SET date_last_updated=GETDATE(), num_processed=@rows, num_remaining=@remainingrowcount WHERE id=@affidavit_delivery_run_id

				DROP TABLE temp_db_backup.dbo.affidavit_delivery_calculation_regional;
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
				temp_db_backup.dbo.affidavit_delivery_queue
			FROM
				temp_db_backup.dbo.affidavit_delivery_queue d
				JOIN temp_db_backup.dbo.affidavit_delivery_calculation_batch b ON b.audience_id=d.audience_id
					AND b.affidavit_id=d.affidavit_id 
			
			-- clear processing table for next batch
			TRUNCATE TABLE temp_db_backup.dbo.affidavit_delivery_calculation_batch;

			IF @remainingrowcount <= 0 BREAK
		END
	
		SET @textRows = LEFT(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1), LEN(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1))-3);
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Estimating delivery for %s is done. %s rows added to	 table.', 0, 1, @textTimestamp, @codeMediaMonth, @textRows) WITH NOWAIT;
		IF @affidavit_delivery_run_id IS NOT NULL
			UPDATE dbo.affidavit_delivery_runs SET date_last_updated=GETDATE(), date_completed=GETDATE(), time_to_calculate_deliveries=DATEDIFF(second, @timestamp, GETDATE()) WHERE id=@affidavit_delivery_run_id
		IF @affidavit_delivery_run_id IS NOT NULL 
		BEGIN
			WAITFOR DELAY '000:00:00.500'
			INSERT INTO dbo.affidavit_delivery_run_messages SELECT @affidavit_delivery_run_id,getdate(),'Processing completed successfully'
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

	IF OBJECT_ID('temp_db_backup.dbo.affidavit_delivery_queue') IS NOT NULL DROP TABLE temp_db_backup.dbo.affidavit_delivery_queue;
	IF OBJECT_ID('temp_db_backup.dbo.affidavit_delivery_calculation_batch') IS NOT NULL DROP TABLE temp_db_backup.dbo.affidavit_delivery_calculation_batch;		
	IF OBJECT_ID('temp_db_backup.dbo.affidavit_delivery_calculation_regional') IS NOT NULL DROP TABLE temp_db_backup.dbo.affidavit_delivery_calculation_regional;
	IF OBJECT_ID('tempdb..#nielsen_network_ratings_map') IS NOT NULL DROP TABLE #nielsen_network_ratings_map;
	IF OBJECT_ID('tempdb..#market_breaks_counties') IS NOT NULL DROP TABLE #market_breaks_counties;
	IF OBJECT_ID('tempdb..#market_breaks') IS NOT NULL DROP TABLE #market_breaks;
	IF OBJECT_ID('tempdb..#nielsen_nad') IS NOT NULL DROP TABLE #nielsen_nad;

	SET NOCOUNT OFF;
END
