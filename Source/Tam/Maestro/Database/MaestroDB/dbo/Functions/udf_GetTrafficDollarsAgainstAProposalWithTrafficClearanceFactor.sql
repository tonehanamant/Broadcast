CREATE FUNCTION [dbo].udf_GetTrafficDollarsAgainstAProposalWithTrafficClearanceFactor
(
      @proposal_id INT
)
RETURNS MONEY
AS
BEGIN
 DECLARE @return FLOAT;
      SET @return = (
            SELECT 
				ISNULL(SUM(traffic_dollars_allocated * dbo.udf_GetTrafficClearanceFactor(traffic_dollars_allocation_lookup.traffic_id,t.start_date)), 0)
			FROM
				traffic_dollars_allocation_lookup (NOLOCK)
				join traffic t (NOLOCK) on t.id = traffic_dollars_allocation_lookup.traffic_id
			WHERE
				proposal_id = @proposal_id
	  )
      RETURN @return;
END
