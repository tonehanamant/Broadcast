-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 8/20/2014
-- Description:	Gets the cumulative cap details for a given proposal.
-- =============================================
-- usp_TCS_GetCumulativeTrafficCapDetails 54668
CREATE PROCEDURE [dbo].[usp_TCS_GetCumulativeTrafficCapDetails]
	@proposal_id INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @is_married AS BIT
	SET @is_married = (
		SELECT CASE WHEN COUNT(1)>0 THEN 1 ELSE 0 END FROM proposal_proposals pp (NOLOCK) WHERE pp.child_proposal_id=@proposal_id
	);

	IF @is_married = 1
	BEGIN
		SELECT
			r.name 'release',
			t.id 'traffic_id', 
			dp.product,
			t.start_date, 
			t.end_date, 
			CAST(tda.release_dollars_allocated * dbo.udf_GetReleaseClearanceFactor(t.id,t.start_date) AS MONEY) 'release_dollars_allocated', 
			dbo.udf_GetReleaseAmountByTrafficId(t.id) 'total_release_dollars_of_traffic_order', 
			CAST(CASE WHEN dbo.udf_GetReleaseAmountByTrafficId(t.id) > 0 THEN tda.release_dollars_allocated / dbo.udf_GetReleaseAmountByTrafficId(t.id) * 100.0 ELSE 0 END AS FLOAT) 'percentage',
			@is_married 'is_married'
		FROM 
			dbo.traffic_dollars_allocation_lookup tda (NOLOCK) 
			JOIN dbo.traffic t (NOLOCK) ON tda.traffic_id = t.id
			JOIN dbo.traffic_proposals tp (NOLOCK) ON tp.traffic_id = t.id
			JOIN dbo.proposal_proposals pp (NOLOCK) ON pp.parent_proposal_id = tp.proposal_id and pp.child_proposal_id = tda.proposal_id
			JOIN dbo.uvw_display_proposals dp (NOLOCK) ON dp.id = pp.parent_proposal_id
			JOIN dbo.releases r (NOLOCK) ON r.id=t.release_id
		WHERE 
			tda.proposal_id = @proposal_id 
			AND tda.release_dollars_allocated > 0
		ORDER BY
			t.start_date,
			r.name
	END
	ELSE
	BEGIN
		SELECT
			r.name 'release',
			t.id 'traffic_id', 
			dp.product,
			t.start_date, 
			t.end_date, 
			CAST(tda.release_dollars_allocated * dbo.udf_GetReleaseClearanceFactor(t.id,t.start_date) AS MONEY) 'release_dollars_allocated', 
			dbo.udf_GetReleaseAmountByTrafficId(t.id) 'total_release_dollars_of_traffic_order', 
			CAST(CASE WHEN dbo.udf_GetReleaseAmountByTrafficId(t.id) > 0 THEN tda.release_dollars_allocated / dbo.udf_GetReleaseAmountByTrafficId(t.id) * 100.0 ELSE 0 END AS FLOAT) 'percentage',
			@is_married 'is_married'
		FROM 
			dbo.traffic_dollars_allocation_lookup tda (NOLOCK) 
			JOIN dbo.traffic t (NOLOCK) ON tda.traffic_id = t.id
			JOIN dbo.traffic_proposals tp (NOLOCK) ON tp.traffic_id = t.id
			JOIN dbo.uvw_display_proposals dp (NOLOCK) ON dp.id = tp.proposal_id
			JOIN dbo.releases r (NOLOCK) ON r.id=t.release_id
		WHERE 
			tda.proposal_id = @proposal_id 
			AND tda.release_dollars_allocated > 0
		ORDER BY
			t.start_date,
			r.name
	END
END
