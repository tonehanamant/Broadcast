-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 2/11/2014
-- Description:	Checks if contracted hh delivery, primary demo delivery, units, and total cost by network match the work sheet of the ordered parent plan for the designated media month.
-- =============================================
-- SELECT dbo.udf_DoesPostingPlanContractTotalsMatchTheOrder(44893)
CREATE FUNCTION udf_DoesPostingPlanContractTotalsMatchTheOrder
(
	@posting_plan_proposal_id INT
)
RETURNS BIT
AS
BEGIN
	DECLARE @return BIT;

	WITH posting_plan_contract AS (
		SELECT
			pd.network_id,
			SUM(dbo.GetProposalDetailTotalDeliveryUnEQ(pd.id,31)) 'hh_delivery',
			SUM(dbo.GetProposalDetailTotalDeliveryUnEQ(pd.id,pa.audience_id)) 'demo_delivery',
			SUM(pd.num_spots) 'units',
			SUM(pd.num_spots * pd.proposal_rate) 'total_cost'
		FROM
			proposals p (NOLOCK)
			JOIN proposal_details pd (NOLOCK) ON pd.proposal_id=p.id
			LEFT JOIN proposal_audiences pa (NOLOCK) ON pa.proposal_id=p.id AND pa.ordinal=1
		WHERE
			p.id=@posting_plan_proposal_id
		GROUP BY
			pd.network_id
	),
	ordered_plan_monthly_contract AS (
		SELECT
			pd.network_id,
			SUM(dbo.GetProposalDetailDeliveryUnEQ(pd.id,31) * pdw.units) 'hh_delivery',
			SUM(dbo.GetProposalDetailDeliveryUnEQ(pd.id,pa.audience_id) * pdw.units) 'demo_delivery',
			SUM(pdw.units) 'units',
			SUM(pdw.units * pd.proposal_rate) 'total_cost'
		FROM
			proposals p (NOLOCK)
			JOIN proposal_details pd (NOLOCK) ON pd.proposal_id=p.original_proposal_id
			JOIN proposal_detail_worksheets pdw (NOLOCK) ON pdw.proposal_detail_id=pd.id
			JOIN media_weeks mw (NOLOCK) ON mw.id=pdw.media_week_id
				AND mw.media_month_id = p.posting_media_month_id
			LEFT JOIN proposal_audiences pa (NOLOCK) ON pa.proposal_id=p.id AND pa.ordinal=1
		WHERE
			p.id=@posting_plan_proposal_id
		GROUP BY
			pd.network_id
	)
	
	SELECT @return = (
		SELECT CASE WHEN COUNT(1)= 
			SUM(
				CASE WHEN ROUND(ppc.hh_delivery,2)=ROUND(opmc.hh_delivery,2) THEN 1 ELSE 0 END 
				& CASE WHEN ROUND(ppc.demo_delivery,2)=ROUND(opmc.demo_delivery,2) THEN 1 ELSE 0 END
				& CASE WHEN ppc.units=opmc.units THEN 1 ELSE 0 END
				& CASE WHEN ppc.total_cost=opmc.total_cost THEN 1 ELSE 0 END
			)
			THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END 'equal'
		FROM 
			posting_plan_contract ppc
			JOIN ordered_plan_monthly_contract opmc ON opmc.network_id=ppc.network_id
		)
	
	RETURN @return;
END
