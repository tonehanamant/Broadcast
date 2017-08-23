/****** Object:  Function [dbo].[udf_GetTrafficDollarsAllocatedByNetworkForMarriedPlan]    Script Date: 02/12/2014 10:16:38 ******/
CREATE FUNCTION [dbo].[udf_GetTrafficDollarsAllocatedByNetworkForMarriedPlan]
(
      @traffic_detail_id INT,
      @source_proposal_id INT
)
RETURNS MONEY
AS
BEGIN
	DECLARE @return AS MONEY
	DECLARE @proposal_cpm1 MONEY;
	DECLARE @proposal_cpm2 MONEY;
	DECLARE @proposal_id1 INT;
	DECLARE @proposal_id2 INT;
	  	  
	SELECT 
		@proposal_cpm1 = proposal_cpm1, 
		@proposal_cpm2 = proposal_cpm2,
		@proposal_id1 = proposal_id1,
		@proposal_id2 = proposal_id2
	FROM
		dbo.udf_TCS_GetMarriedTrafficRateTableByTrafficDetailId (@traffic_detail_id);
      

      SET @return = (
            SELECT 
                  CASE WHEN @source_proposal_id = @proposal_id1 THEN
                        CASE WHEN (@proposal_cpm1 + @proposal_cpm2) > 0 THEN
                              CAST(((@proposal_cpm1 / (@proposal_cpm1 + @proposal_cpm2)) * td.traffic_amount) * ((pp.rotation_percentage / 100.0)) AS MONEY)
                        ELSE
                              0.0
                        END
                  ELSE
                        CASE WHEN (@proposal_cpm1 + @proposal_cpm2) > 0 THEN
                              CAST(((@proposal_cpm2 / (@proposal_cpm1 + @proposal_cpm2)) * td.traffic_amount) * ((pp.rotation_percentage / 100.0)) AS MONEY)
                        ELSE 
                              0.0
                        END
                  END
            FROM
                  dbo.traffic_details td (NOLOCK)
				  JOIN dbo.traffic_proposals tp (NOLOCK) on tp.traffic_id = td.traffic_id
				  JOIN dbo.proposal_proposals pp (NOLOCK) ON tp.proposal_id = pp.parent_proposal_id 
            WHERE
                  pp.child_proposal_id = @source_proposal_id AND td.id = @traffic_detail_id
      )
      RETURN @return;
END
