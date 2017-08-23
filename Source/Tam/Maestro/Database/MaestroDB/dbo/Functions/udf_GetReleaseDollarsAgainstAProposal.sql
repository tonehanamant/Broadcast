CREATE FUNCTION [dbo].[udf_GetReleaseDollarsAgainstAProposal]
(
      @proposal_id INT
)
RETURNS MONEY
AS
BEGIN
 DECLARE @return FLOAT;
      SET @return = (
            SELECT 
				ISNULL(SUM(release_dollars_allocated), 0) 
			FROM
				traffic_dollars_allocation_lookup (NOLOCK)
			WHERE
				proposal_id = @proposal_id
	  )
      RETURN @return;
END
