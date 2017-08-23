CREATE FUNCTION [dbo].[udf_GetCumulativeTrafficCapOverageByProposal]
(
      @proposal_id INT
)
RETURNS @return TABLE
(
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
      declare @effective_date datetime;
      declare @release_dollars_without_clearance money;
      declare @release_dollars money;
      declare @traffic_dollars money;
      
      SELECT @effective_date = p.start_date FROM proposals p (NOLOCK) WHERE p.id=@proposal_id;
      SELECT @proposal_discount = CAST(p.value AS FLOAT) FROM properties p (NOLOCK) WHERE p.name='proposal_discount_factor_for_traffic_overage'; -- Factor out agency commission
	  SELECT @traffic_dollars = dbo.udf_GetTrafficDollarsAgainstAProposalWithTrafficClearanceFactor(@proposal_id);
	  SELECT @release_dollars_without_clearance = dbo.udf_GetReleaseDollarsAgainstAProposal(@proposal_id);
	  SELECT @release_dollars = dbo.udf_GetReleaseDollarsAgainstAProposalWithReleaseClearanceFactor(@proposal_id);

      INSERT INTO @return
            SELECT 
                  p.id 'proposal id',
                  p.print_title 'proposal_title',
                  (p.total_gross_cost) 'contract_dollars',
                  (p.total_gross_cost * @proposal_discount) 'contract_cap_dollars',
                  @traffic_dollars 'traffic_dollars',
                  @traffic_dollars - (p.total_gross_cost * @proposal_discount) 'traffic_overage_dollars',
                  @release_dollars_without_clearance 'release_dollars_without_clearance',
                  @release_dollars 'release_dollars',
                  @release_dollars - (p.total_gross_cost * @proposal_discount) 'release_overage_dollars',
                  CASE WHEN @traffic_dollars > (p.total_gross_cost * @proposal_discount) THEN
                        'OVER CAP'
                  ELSE
                        'Passed'
                  END 'traffic_overage_status',
                  CASE WHEN @traffic_dollars > CAST((p.total_gross_cost * @proposal_discount) AS MONEY) THEN
                        CAST(1 AS BIT)
                  ELSE
                        CAST(0 AS BIT)
                  END 'is_traffic_over',
                  CASE WHEN @release_dollars > CAST((p.total_gross_cost * @proposal_discount) AS MONEY) THEN
                        'OVER CAP'
                  ELSE
                        'Passed'
                  END 'release_overage_status',
                  CASE WHEN @release_dollars > CAST((p.total_gross_cost * @proposal_discount) AS MONEY) THEN
                        CAST(1 AS BIT)
                  ELSE
                        CAST(0 AS BIT)
                  END 'is_release_over',
                  CASE WHEN tca.proposal_id IS NOT NULL AND @traffic_dollars <= CAST(tca.approval_amount AS MONEY) THEN
                        CAST(1 AS BIT)
                  ELSE
                        CAST(0 AS BIT)
                  END 'traffic_is_overruled',
                  CASE WHEN tca.proposal_id IS NOT NULL AND @release_dollars <= CAST(tca.approval_amount AS MONEY) THEN
                        CAST(1 AS BIT)
                  ELSE
                        CAST(0 AS BIT)
                  END 'release_is_overruled',
				  tca.approval_amount,
				  p.flight_text
            FROM
                  proposals p (NOLOCK)
                  LEFT JOIN traffic_cap_cumulative_override_approvals tca (NOLOCK) ON p.id = tca.proposal_id 
            WHERE
                  p.id = @proposal_id;

      RETURN;
END
