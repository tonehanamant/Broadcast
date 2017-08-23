
/***************************************************************************************************
** Date			Author			Description	
** ---------	----------		--------------------------------------------------------------------
** XX/XX/XXXX	XXXXX			Created Function
** 07/28/2015	Abdul Sukkur 	Task-8626-Statistical Tables for Married Plans to Improve Performance.
*****************************************************************************************************/
CREATE FUNCTION [dbo].[udf_GetMarriedReleaseAmountByTrafficIdAndProposalID]
(
      @traffic_id INT,
	  @proposal_id INT
)
RETURNS MONEY
AS
BEGIN
	  DECLARE @return AS MONEY;

	  	select @return = SUM (a.amount) from
		 ( SELECT (case pp.ordinal when 0 then ISNULL(td.release_amount1,0) else  ISNULL(td.release_amount2,0) end)  * (pp.rotation_percentage / 100.0) as amount
		  FROM dbo.traffic_details td (NOLOCK)
		  	join dbo.traffic_proposals tp (NOLOCK) on td.traffic_id = tp.traffic_id
			JOIN proposal_proposals pp (NOLOCK) ON pp.parent_proposal_id=tp.proposal_id
			WHERE
			pp.child_proposal_id=@proposal_id
			AND tp.traffic_id=@traffic_id ) as a



      RETURN @return;
END

