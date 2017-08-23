CREATE FUNCTION [dbo].[udf_GetCumulativeTrafficCapOverage]
(
      @traffic_id INT
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

      SELECT @proposal_discount = CAST(p.value AS FLOAT) FROM properties p (NOLOCK) WHERE p.name='proposal_discount_factor_for_traffic_overage'; -- Factor out agency commission

      INSERT INTO @return
            SELECT DISTINCT 
                  p.id 'proposal id',
                  p.print_title 'proposal_title',
                  (p.total_gross_cost) 'contract_dollars',
                  (p.total_gross_cost * @proposal_discount) 'contract_cap_dollars',
                  dbo.udf_GetTrafficDollarsAgainstAProposalWithTrafficClearanceFactor(p.id) 'traffic_dollars',
                  dbo.udf_GetTrafficDollarsAgainstAProposalWithTrafficClearanceFactor(p.id) - (p.total_gross_cost * @proposal_discount) 'traffic_overage_dollars',
                  dbo.udf_GetReleaseDollarsAgainstAProposal(p.id) 'release_dollars_without_clearance',
                  dbo.udf_GetReleaseDollarsAgainstAProposalWithReleaseClearanceFactor(p.id) 'release_dollars',
                  dbo.udf_GetReleaseDollarsAgainstAProposalWithReleaseClearanceFactor(p.id) - (p.total_gross_cost * @proposal_discount) 'release_overage_dollars',
                  CASE WHEN dbo.udf_GetTrafficDollarsAgainstAProposalWithTrafficClearanceFactor(p.id) > (p.total_gross_cost * @proposal_discount) THEN
                        'OVER CAP'
                  ELSE
                        'Passed'
                  END 'traffic_overage_status',
                  CASE WHEN dbo.udf_GetTrafficDollarsAgainstAProposalWithTrafficClearanceFactor(p.id) > CAST((p.total_gross_cost * @proposal_discount) AS MONEY) THEN
                        CAST(1 AS BIT)
                  ELSE
                        CAST(0 AS BIT)
                  END 'is_traffic_over',
                  CASE WHEN dbo.udf_GetReleaseDollarsAgainstAProposalWithReleaseClearanceFactor(p.id) > CAST((p.total_gross_cost * @proposal_discount) AS MONEY) THEN
                        'OVER CAP'
                  ELSE
                        'Passed'
                  END 'release_overage_status',
                  CASE WHEN dbo.udf_GetReleaseDollarsAgainstAProposalWithReleaseClearanceFactor(p.id) > CAST((p.total_gross_cost * @proposal_discount) AS MONEY) THEN
                        CAST(1 AS BIT)
                  ELSE
                        CAST(0 AS BIT)
                  END 'is_release_over',
                  CASE WHEN tca.proposal_id IS NOT NULL AND dbo.udf_GetTrafficDollarsAgainstAProposalWithTrafficClearanceFactor(p.id) <= CAST(tca.approval_amount AS MONEY) THEN
                        CAST(1 AS BIT)
                  ELSE
                        CAST(0 AS BIT)
                  END 'traffic_is_overruled',
                  CASE WHEN tca.proposal_id IS NOT NULL AND dbo.udf_GetReleaseDollarsAgainstAProposalWithReleaseClearanceFactor(p.id) <= CAST(tca.approval_amount AS MONEY) THEN
                        CAST(1 AS BIT)
                  ELSE
                        CAST(0 AS BIT)
                  END 'release_is_overruled',
				  tca.approval_amount 'cumulative_approval_amount',
				  p.flight_text
            FROM
                  traffic_dollars_allocation_lookup tda (NOLOCK)
                  JOIN proposals p (NOLOCK) on p.id = tda.proposal_id
                  LEFT JOIN traffic_cap_cumulative_override_approvals tca (NOLOCK) ON p.id = tca.proposal_id 
            WHERE
                  tda.traffic_id = @traffic_id;

      RETURN;
END
