CREATE FUNCTION [dbo].[udf_GetMonthlyTrafficCapOverage]
(
	@traffic_id INT
)
RETURNS @return TABLE
(
    traffic_id INT,
    traffic_name VARCHAR(MAX),
    traffic_flight VARCHAR(1027),
	traffic_monthly_approval_amount MONEY,
    proposal_id INT,
    proposal_title VARCHAR(MAX),
    contract_dollars MONEY,
    contract_cap_dollars MONEY,
    traffic_dollars MONEY,
    traffic_overage_dollars MONEY,
    release_dollars_without_clearance MONEY,
    release_dollars MONEY,
    release_overage_dollars MONEY,
    traffic_overage_status VARCHAR(MAX),
    is_traffic_over BIT,
	release_overage_status VARCHAR(MAX),
    is_release_over BIT,
	traffic_is_overruled BIT,
	release_is_overruled BIT,
	cumulative_approval_amount MONEY,
	proposal_flight_text VARCHAR(1027)
)
AS
BEGIN
	declare @proposal_discount float;
	declare @traffic_clearance float;
	declare @release_clearance float;
	declare @effective_date datetime;

	SELECT @effective_date = t.start_date FROM traffic t (NOLOCK) WHERE t.id=@traffic_id;
	SELECT @proposal_discount = CAST(p.value AS FLOAT) FROM properties p (NOLOCK) WHERE p.name='proposal_discount_factor_for_traffic_overage'; -- Factor out agency commission
	set @traffic_clearance = dbo.udf_GetTrafficClearanceFactor(@traffic_id,@effective_date); --Factor in clearance estimates
	set @release_clearance = dbo.udf_GetReleaseClearanceFactor(@traffic_id,@effective_date); --Factor in clearance estimates

	INSERT INTO @return
		SELECT 
			t.id 'traffic_id',
			t.name 'traffic_name',
			dbo.udf_GetTrafficFlightText(t.id) 'traffic_flight_text',
			tca.approval_amount 'traffic_monthly_approval_amount',
			p.id 'proposal id',
			CASE WHEN p.print_title IS NULL OR LEN(p.print_title)=0 THEN p.name ELSE p.print_title END 'proposal_title',
			SUM(tda.proposal_dollars_allocated) 'contract_dollars',
			SUM(tda.proposal_dollars_allocated) * @proposal_discount 'contract_cap_dollars',
			SUM(tda.traffic_dollars_allocated) * @traffic_clearance 'traffic_dollars',
			(SUM(tda.traffic_dollars_allocated) * @traffic_clearance) - (SUM(tda.proposal_dollars_allocated) * @proposal_discount) 'traffic_overage_dollars',
			SUM(tda.release_dollars_allocated) 'release_dollars_without_clearance',
			SUM(tda.release_dollars_allocated) * @release_clearance 'release_dollars',
			(SUM(tda.release_dollars_allocated) * @release_clearance) - (SUM(tda.proposal_dollars_allocated) * @proposal_discount) 'release_overage_dollars',
			CASE WHEN (sum(tda.traffic_dollars_allocated) * @traffic_clearance) > (sum(tda.proposal_dollars_allocated) * @proposal_discount) THEN
				'OVER CAP'
			ELSE
				'Passed'
			END 'traffic_overage_status',
			CASE WHEN CAST((sum(tda.traffic_dollars_allocated) * @traffic_clearance) AS MONEY) > CAST((sum(tda.proposal_dollars_allocated) * @proposal_discount) AS MONEY) THEN
				CAST(1 AS BIT)
			ELSE
				CAST(0 AS BIT)
			END 'is_traffic_over',
		
			CASE WHEN CAST((sum(tda.release_dollars_allocated) * @release_clearance) AS MONEY) > CAST((sum(tda.proposal_dollars_allocated) * @proposal_discount) AS MONEY) THEN
				'OVER CAP'
			ELSE
				'Passed'
			END 'release_overage_status',
			CASE WHEN CAST((sum(tda.release_dollars_allocated) * @release_clearance) AS MONEY) > CAST((sum(tda.proposal_dollars_allocated) * @proposal_discount) AS MONEY) THEN
				CAST(1 AS BIT)
			ELSE
				CAST(0 AS BIT)
			END 'is_release_over',
			CASE WHEN tca.traffic_id IS NOT NULL AND CAST(SUM(tda.traffic_dollars_allocated) * @traffic_clearance AS MONEY) <= CAST(tca.approval_amount AS MONEY) THEN
				CAST(1 AS BIT)
			ELSE
				CAST(0 AS BIT)
			END 'traffic_is_overruled',
			CASE WHEN tca.traffic_id IS NOT NULL AND CAST(SUM(tda.release_dollars_allocated) * @release_clearance AS MONEY) <= CAST(tca.approval_amount AS MONEY) THEN
				CAST(1 AS BIT)
			ELSE
				CAST(0 AS BIT)
			END 'release_is_overruled',
			NULL 'cumulative_approval_amount',
			p.flight_text 'proposal_flight_text'
		FROM
			traffic_dollars_allocation_lookup tda (NOLOCK)
			JOIN traffic t (NOLOCK) ON t.id=tda.traffic_id
			JOIN traffic_proposals tp on tp.traffic_id = t.id
			JOIN proposals p (NOLOCK) on p.id = tp.proposal_id
			LEFT JOIN traffic_cap_monthly_override_approvals tca (NOLOCK) ON tca.traffic_id = t.id
			LEFT JOIN notes n (NOLOCK) ON n.id=t.internal_note_id
		WHERE
			tda.traffic_id = @traffic_id
		GROUP BY
			t.id,tca.traffic_id,tca.approval_amount,t.name,p.id,p.name,p.print_title,CAST(n.comment AS VARCHAR(2047)),p.flight_text

	RETURN;
END
