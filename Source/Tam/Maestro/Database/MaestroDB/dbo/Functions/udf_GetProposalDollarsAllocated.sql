
/***************************************************************************************************
** Date			Author			Description	
** ---------	----------		--------------------------------------------------------------------
** XX/XX/XXXX	XXXXX			Created Function
** 07/28/2015	Abdul Sukkur 	Task-8626-Statistical Tables for Married Plans to Improve Performance.
*****************************************************************************************************/
CREATE FUNCTION [dbo].[udf_GetProposalDollarsAllocated]
(
      @traffic_id INT,
      @proposal_id INT
)
RETURNS MONEY
AS
BEGIN

    DECLARE @return AS MONEY;

	IF dbo.udf_IsTrafficMarried(@traffic_id) = 1
	BEGIN
		SELECT
			@return = SUM(ISNULL(pd.proposal_rate,0) * pdw.units)
		FROM
			dbo.traffic_proposals tp (NOLOCK)
			JOIN proposal_proposals pp (NOLOCK) ON pp.parent_proposal_id=tp.proposal_id
			JOIN dbo.traffic t (NOLOCK) ON t.id=tp.traffic_id
			JOIN dbo.proposal_details pd (NOLOCK) ON pd.proposal_id=pp.child_proposal_id
			JOIN dbo.proposal_detail_worksheets pdw (NOLOCK) ON pdw.proposal_detail_id=pd.id
			JOIN dbo.media_weeks pmw (NOLOCK) ON pmw.id=pdw.media_week_id
				AND (pmw.start_date <= t.end_date AND pmw.end_date >= t.start_date)
		WHERE
			pp.child_proposal_id=@proposal_id
			AND tp.traffic_id=@traffic_id
	END
	ELSE
	BEGIN
		SELECT
			@return = SUM(ISNULL(pd.proposal_rate,0) * pdw.units)
		FROM
			dbo.traffic_proposals tp (NOLOCK)
			JOIN dbo.traffic t (NOLOCK) ON t.id=tp.traffic_id
			JOIN dbo.proposal_details pd (NOLOCK) ON pd.proposal_id=tp.proposal_id
			JOIN dbo.proposal_detail_worksheets pdw (NOLOCK) ON pdw.proposal_detail_id=pd.id
			JOIN dbo.media_weeks pmw (NOLOCK) ON pmw.id=pdw.media_week_id
				AND (pmw.start_date <= t.end_date AND pmw.end_date >= t.start_date)
		WHERE
			tp.proposal_id=@proposal_id
			AND tp.traffic_id=@traffic_id
    END
	RETURN @return;
END
