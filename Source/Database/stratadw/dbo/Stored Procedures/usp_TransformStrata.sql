CREATE   PROCEDURE [dbo].[usp_TransformStrata]
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		-- check if there's data
		IF (SELECT COUNT(1) FROM [dbo].[strata_estimate]) > 0
		BEGIN
			BEGIN TRANSACTION;

			TRUNCATE TABLE stratadw.dbo.estimate_order_details;
			TRUNCATE TABLE stratadw.dbo.estimates;

			DROP TABLE IF EXISTS #duplicate_estimate_ids;
			CREATE TABLE #duplicate_estimate_ids (estimate_id BIGINT NOT NULL);
			INSERT INTO #duplicate_estimate_ids
				SELECT DISTINCT
					e.ESTIMATEID
				FROM
					nsi_staging.dbo.strata_estimate e
				GROUP BY
					e.ESTIMATEID
				HAVING 
					COUNT(1)>1;

			-- ETL
			INSERT INTO stratadw.dbo.estimates ([estimate_id], [flight_start_date], [flight_end_date], [description], [estimate_group], [product_name])
				SELECT DISTINCT
					e.ESTIMATEID,
					e.FLIGHTSTARTDATE,
					e.FLIGHTENDDATE,
					e.ESTIMATEDESCRIPTION,
					e.ESTIMATEGROUP,
					e.PRODUCTNAME
				FROM
					nsi_staging.dbo.strata_estimate e
				WHERE
					e.ESTIMATEID NOT IN (
						SELECT estimate_id FROM #duplicate_estimate_ids
					);
					
			-- ETL
			INSERT INTO stratadw.dbo.estimate_order_details ([estimate_id], [source_market_name], [market_code], [order_date], [order_start_date], [order_end_date], [days_of_week], [order_start_time_seconds], [order_end_time_seconds], [affiliation], [station_call_letters], [survey_name], [vendor_name], [program_name], [source_audience], [audience_id], [audience_ordinal], [impressions], [rating])
				SELECT DISTINCT
					od.ESTIMATEID,
					od.MARKETNAME,
					smm.market_code,
					od.ORDERDATE,
					od.ORDERSTARTDATE,
					od.ORDERENDDATE,
					od.DAYSOFWEEK,
					od.order_start_time_secs,
					od.order_end_time_secs,
					ISNULL(od.AFFILIATION, ''),
					ISNULL(od.STATIONNAME, ''),
					ISNULL(od.SURVEYNAME, ''),
					ISNULL(od.VENDORNAME, ''),
					ISNULL(od.PROGRAM, ''),
					od.DEMO,
					a.id,
					od.DEMOORDERID,
					od.IMP000 * 1000,
					od.RTG / 100
				FROM
					nsi_staging.dbo.strata_orderdetails od
					LEFT JOIN nsi_staging.dbo.strata_market_mappings smm ON smm.marketid=od.MARKETID
					LEFT JOIN broadcast.dbo.audiences a WITH (NOLOCK) ON a.name=od.DEMO
				WHERE
					od.ESTIMATEID NOT IN (
						SELECT estimate_id FROM #duplicate_estimate_ids
					);

			TRUNCATE TABLE nsi_staging.dbo.strata_orderdetails;
			TRUNCATE TABLE nsi_staging.dbo.strata_estimate;
			TRUNCATE TABLE nsi_staging.dbo.strata_market_mappings;

			UPDATE nsi_staging.dbo.strata_status SET status='Ready' WHERE id=1;

			COMMIT TRANSACTION;
		END
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION;

		DECLARE @ErrorNumber INT;
		DECLARE @ErrorSeverity INT;
		DECLARE @ErrorState INT;
		DECLARE @ErrorProcedure NVARCHAR(200);
		DECLARE @ErrorLine INT;
		DECLARE @ErrorMessage NVARCHAR(4000);

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
	END CATCH
END
