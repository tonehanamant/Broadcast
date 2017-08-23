-- =============================================
-- Author:		Stephen DeFusco w/Matt Plourde
-- Create date: 8/17/2016
-- Description:	Given a media month, this procedure populates inventory.dbo.load_inputs for this media month from the prespective of the last 18 months.
--				For example, it's like saying what was contracted for 0716 at the end of 0716, at the end of 0616, at the end of 0516, and so on to 18 months.
-- =============================================
-- EXEC dbo.usp_ICS_ProcessInventoryLoadInputs 417
-- =============================================
CREATE PROCEDURE [dbo].[usp_ICS_ProcessInventoryLoadInputs]
	@media_month_id SMALLINT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @effective_media_month_id INT;
	DECLARE @max_horizon_months INT = 18;
	DECLARE @effective_media_month_end_date DATE;
	DECLARE @horizon_counter INT = 0;
	DECLARE @media_month VARCHAR(4);
	
	SELECT @media_month = mm.media_month FROM media_months mm WHERE mm.id=@media_month_id;

	-- clear month if it already exists
	IF (SELECT COUNT(1) FROM inventory.dbo.load_inputs lf WHERE lf.media_month_id=@media_month_id) > 0
		DELETE FROM inventory.dbo.load_inputs WHERE media_month_id=@media_month_id;

	-- clear month if it already exists
	IF (SELECT COUNT(1) FROM inventory.dbo.load_inputs_final lf WHERE lf.media_month=@media_month) > 0
		DELETE FROM inventory.dbo.load_inputs_final WHERE media_month=@media_month;

    CREATE TABLE #reserved_inventory_holder(
		ProposalId INT,
		UniqueProposalDetailId INT,
		MediaMonthId INT,
		MediaWeekId INT,
		NetworkId INT,
		ComponentDaypartId INT,
		ProposalStatusId INT,
		HhEqCpmStart REAL,
		HhEqCpmEnd REAL,
		ContractedSubscribers INT
	);

	WHILE @horizon_counter < @max_horizon_months BEGIN
		SELECT 
			@effective_media_month_id = mm.id,
			@effective_media_month_end_date = mm.end_date
		FROM 
			media_months mm 
		WHERE 
			mm.id=dbo.udf_CalculateFutureMediaMonthId(@media_month_id, @horizon_counter * -1);

		INSERT INTO #reserved_inventory_holder
			EXEC usp_ICS_GetReservedProposalInventoryForMediaMonth @media_month_id, @effective_media_month_end_date;

		-- if the media month being processed is the effective media month include the "proposal_detail_id" in the aggregation, otherwise use 0.
		--   this is specifically requested by the Data Science team (see Matthew P for more details).
		IF @effective_media_month_id = @media_month_id
		BEGIN
			INSERT INTO inventory.dbo.load_inputs (media_month_id, proposal_id, effective_media_month_id, media_week_id, network_id, daypart_id, hh_eq_cpm_start, hh_eq_cpm_end, subscribers)
				SELECT 
					MediaMonthId, ProposalId, @effective_media_month_id, MediaWeekId, NetworkId, ComponentDaypartId, HhEqCpmStart, HhEqCpmEnd, SUM(ContractedSubscribers)
				FROM 
					#reserved_inventory_holder
				GROUP BY 
					MediaMonthId, ProposalId, MediaWeekId, NetworkId, ComponentDaypartId, HhEqCpmStart, HhEqCpmEnd
		END
		ELSE
		BEGIN
			INSERT INTO inventory.dbo.load_inputs (media_month_id, proposal_id, effective_media_month_id, media_week_id, network_id, daypart_id, hh_eq_cpm_start, hh_eq_cpm_end, subscribers)
				SELECT 
					MediaMonthId, 0, @effective_media_month_id, MediaWeekId, NetworkId, ComponentDaypartId, HhEqCpmStart, HhEqCpmEnd, SUM(ContractedSubscribers)
				FROM 
					#reserved_inventory_holder
				GROUP BY 
					MediaMonthId, MediaWeekId, NetworkId, ComponentDaypartId, HhEqCpmStart, HhEqCpmEnd
		END
		
		TRUNCATE TABLE #reserved_inventory_holder;

		SET @horizon_counter = @horizon_counter + 1;
	END

	DROP TABLE #reserved_inventory_holder;


	WITH t1 AS ( 
		SELECT 
			proposal_id,
			ef_mm.media_month 'effective_media_month',
			mm.media_month,
			mm.media_month_index,
			week_number,
			CASE WHEN d.mon = 1 THEN 0 ELSE 1 END 'week_part',
			FLOOR(d.end_time / 3600) * 3600 'seconds',
			network_id,
			CEILING(hh_eq_cpm_end / .25) * .25 'cpm',
			CAST(subscribers as REAL) 'subscribers'
		FROM 
			inventory.dbo.load_inputs li
			JOIN vw_ccc_daypart d ON d.id=daypart_id
			JOIN inventory.dbo.media_months_ix mm ON media_month_id = mm.id
			JOIN inventory.dbo.media_months_ix ef_mm ON effective_media_month_id = ef_mm.id
			INNER JOIN media_weeks mw ON media_week_id = mw.id
		WHERE
			li.media_month_id=@media_month_id
	)
	INSERT INTO inventory.dbo.load_inputs_final (proposal_id, effective_media_month, media_month, media_month_index, week_number, week_part, seconds, network_id, cpm, subscribers)
		SELECT 
			proposal_id, effective_media_month, media_month, media_month_index, week_number, week_part, seconds, network_id, cpm, SUM(subscribers) 'subscribers' FROM t1
		GROUP BY 
			proposal_id, effective_media_month, media_month, media_month_index, week_number, week_part, seconds, network_id, cpm
END