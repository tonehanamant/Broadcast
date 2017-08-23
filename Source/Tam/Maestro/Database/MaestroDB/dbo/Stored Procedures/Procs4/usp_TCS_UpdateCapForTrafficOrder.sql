
/***************************************************************************************************
** Date			Author			Description	
** ---------	----------		--------------------------------------------------------------------
** XX/XX/XXXX	XXXXX			Created SP.Calculate and update traffic order cap check
** 07/28/2015	Abdul Sukkur 	Task-8626-Statistical Tables for Married Plans to Improve Performance.Includeded update for traffic_amount1,2 &release_amount1 &2 
** 10/22/2015   Joe Jacobs		Added in checks on plan type to work with new type of traffic orders
** 10/27/2015   Sankar Krishnan Modified spot_cost to spot_cost1
** 11/18/2015   Abdul Sukkur 	#408-Create Married plans fixed and non fixed(BE). Update spotcost
*****************************************************************************************************/
CREATE PROCEDURE [dbo].[usp_TCS_UpdateCapForTrafficOrder]
	@traffic_id INT
AS
BEGIN
	
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;  
	SET NOCOUNT ON;

	DECLARE @is_married BIT;
	DECLARE @plan_type TINYINT;
	
	SET @is_married = dbo.udf_IsTrafficMarried(@traffic_id);
	SELECT @plan_type = plan_type from traffic where id = @traffic_id;
	
	-- calculate traffic_amount by traffic_detail_id for the given traffic_id
	CREATE TABLE #traffic_amount (traffic_detail_id INT, traffic_amount MONEY);
	
	IF (@plan_type = 0)
	BEGIN
	INSERT INTO #traffic_amount (traffic_detail_id, traffic_amount)
		SELECT
			td.id,
			SUM(ISNULL(tdt.spots * tdt.ordered_spot_cost,0))
		FROM
			traffic_details td (NOLOCK) 
			JOIN traffic_detail_weeks tdw (NOLOCK) ON td.id = tdw.traffic_detail_id
				AND tdw.suspended = 0
			JOIN traffic_detail_topographies tdt (NOLOCK) ON tdt.traffic_detail_week_id = tdw.id
		WHERE
			td.traffic_id = @traffic_id
		GROUP BY
			td.id;
	END
	ELSE
	BEGIN
		INSERT INTO #traffic_amount (traffic_detail_id, traffic_amount)
		SELECT
			td.id,
			SUM(ISNULL(tst.spots * tst.spot_cost,0))
		FROM
			traffic_details td (NOLOCK) 
			JOIN traffic_spot_target_allocation_group stag on stag.traffic_detail_id = td.id 
			JOIN traffic_spot_targets tst on tst.traffic_spot_target_allocation_group_id = stag.id 
		WHERE
			td.traffic_id = @traffic_id and tst.suspended = 0 
		GROUP BY
			td.id;
			
			CREATE TABLE #traffic_temp_cpm (traffic_detail_id INT, CPM1 MONEY, CPM2 MONEY);
			INSERT INTO #traffic_temp_cpm (traffic_detail_id, CPM1, CPM2) 
			SELECT 
				td.id, 
				AVG(ISNULL(stag.traffic_gross_cpm1, 0)), 
				AVG(ISNULL(stag.traffic_gross_cpm2, 0))
			FROM 
				traffic_details td (NOLOCK) 
				JOIN traffic_spot_target_allocation_group stag (NOLOCK) ON stag.traffic_detail_id = td.id
			WHERE
				td.traffic_id = @traffic_id
			GROUP BY
				td.id;
			
		-- ALSO need to update CPMS
		UPDATE 
			td 
		SET 
			td.CPM1 = cpms.CPM1, 
			td.CPM2 = cpms.CPM2
		FROM 
			traffic_details td (NOLOCK) 
			JOIN #traffic_temp_cpm cpms (NOLOCK) ON cpms.traffic_detail_id = td.id
		WHERE
				td.traffic_id = @traffic_id;
		
		DROP TABLE #traffic_temp_cpm;
	
	END
	
	
	-- update traffic_amount in traffic_details
	UPDATE 
		td 
	SET 
		td.traffic_amount = ISNULL(ta.traffic_amount,0) , 
		td.traffic_amount1 = ISNULL((ISNULL(td.CPM1,0) / NULLIF(ISNULL(td.CPM1,0) + ISNULL(td.CPM2,0) ,0)) * ta.traffic_amount ,0),
		td.traffic_amount2 = ISNULL((ISNULL(td.CPM2,0) / NULLIF(ISNULL(td.CPM1,0) + ISNULL(td.CPM2,0) ,0)) * ta.traffic_amount  ,0)
	FROM 
		traffic_details td (NOLOCK) 
		LEFT JOIN #traffic_amount ta (NOLOCK) ON ta.traffic_detail_id = td.id
	WHERE
			td.traffic_id = @traffic_id
	
	DROP TABLE #traffic_amount;

	-- calculate release_amount by traffic_detail_id for the given traffic_id
	CREATE TABLE #release_amount (traffic_detail_id INT, release_amount MONEY);
	IF (@plan_type = 0)
	BEGIN
	INSERT INTO #release_amount (traffic_detail_id, release_amount)
	SELECT 
		tro.traffic_detail_id, 
		SUM(tro.ordered_spot_rate * tro.ordered_spots) 
	FROM 
		traffic_orders tro (NOLOCK)
		JOIN traffic_details td (NOLOCK) ON td.id = tro.traffic_detail_id
			AND td.traffic_id=@traffic_id
		JOIN traffic_detail_weeks tdw (NOLOCK) ON tdw.traffic_detail_id = td.id
			AND tdw.suspended = 0
			AND (tdw.start_date <= tro.end_date AND tdw.end_date >= tro.start_date)
	WHERE 
		tro.on_financial_reports = 1 
		AND tro.active = 1
		AND tro.release_id IS NOT NULL
	GROUP BY
		tro.traffic_detail_id;
	END
	ELSE
	BEGIN 
	INSERT INTO #release_amount (traffic_detail_id, release_amount)
	SELECT 
		tro.traffic_detail_id, 
		SUM(tro.ordered_spot_rate * tro.ordered_spots) 
	FROM 
		traffic_orders tro (NOLOCK)
		JOIN traffic_details td (NOLOCK) ON td.id = tro.traffic_detail_id
			AND td.traffic_id=@traffic_id
		JOIN traffic_spot_target_allocation_group stag on stag.traffic_detail_id = td.id
		JOIN traffic_spot_targets tst on tro.traffic_spot_target_id = tst.id
			AND tst.suspended = 0
			AND (stag.start_date <= tro.end_date AND stag.end_date >= tro.start_date)
	WHERE 
		tro.on_financial_reports = 1 
		AND tro.active = 1
		AND tro.release_id IS NOT NULL
	GROUP BY
		tro.traffic_detail_id;
	END
	
	-- update traffic_details.release_amount
	UPDATE 
		td 
	SET 
		td.release_amount = ISNULL(ra.release_amount,0) , 
		td.release_amount1 = ISNULL((ISNULL(td.CPM1,0) / NULLIF(ISNULL(td.CPM1,0) + ISNULL(td.CPM2,0) ,0)) * ra.release_amount ,0),
		td.release_amount2 = ISNULL((ISNULL(td.CPM2,0) / NULLIF(ISNULL(td.CPM1,0) + ISNULL(td.CPM2,0) ,0)) * ra.release_amount  ,0)
	FROM 
		traffic_details td (NOLOCK)
		LEFT JOIN #release_amount ra (NOLOCK) ON ra.traffic_detail_id = td.id
	WHERE
			td.traffic_id = @traffic_id;

	DROP TABLE #release_amount;


	-- clear out traffic_dollars_allocation_lookup for traffic_id
	DELETE FROM traffic_dollars_allocation_lookup WHERE traffic_id = @traffic_id;

	-- insert new records into traffic_dollars_allocation_lookup (depending on if the traffic_id is married or not)
	IF(@is_married = 0)
	BEGIN
		INSERT INTO traffic_dollars_allocation_lookup(proposal_id, traffic_id, proposal_dollars_allocated, traffic_dollars_allocated, release_dollars_allocated)
			SELECT 
				tp.proposal_id,
				tp.traffic_id,
				ISNULL(dbo.udf_GetProposalDollarsAllocated(tp.traffic_id,tp.proposal_id), 0),
				ISNULL(dbo.udf_GetTrafficAmountByTrafficId(tp.traffic_id), 0),
				ISNULL(dbo.udf_GetReleaseAmountByTrafficId(tp.traffic_id), 0)
			FROM
				dbo.traffic_proposals tp (NOLOCK)
				JOIN dbo.traffic t (NOLOCK) on t.id = tp.traffic_id
			WHERE
				tp.traffic_id = @traffic_id
				AND tp.proposal_id NOT IN (
					SELECT parent_proposal_id FROM proposal_proposals (NOLOCK)
				);
	END
	ELSE
	BEGIN
		INSERT INTO traffic_dollars_allocation_lookup(proposal_id, traffic_id, proposal_dollars_allocated, traffic_dollars_allocated, release_dollars_allocated)
			SELECT 
				pp.child_proposal_id,
				tp.traffic_id,
				ISNULL(dbo.udf_GetProposalDollarsAllocated(tp.traffic_id,pp.child_proposal_id), 0), 
				ISNULL(dbo.udf_GetMarriedTrafficAmountByTrafficIdAndProposalID(tp.traffic_id, pp.child_proposal_id), 0),
				ISNULL(dbo.udf_GetMarriedReleaseAmountByTrafficIdAndProposalID(tp.traffic_id, pp.child_proposal_id), 0)
			FROM
				dbo.traffic_proposals tp (NOLOCK)
				JOIN dbo.traffic t (NOLOCK) on t.id = tp.traffic_id
				JOIN dbo.proposal_proposals pp (NOLOCK) ON pp.parent_proposal_id=tp.proposal_id
			WHERE
				tp.traffic_id = @traffic_id;
	END
END
