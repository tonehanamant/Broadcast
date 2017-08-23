-- =============================================
-- Author:        Nicholas Kheynis
-- Create date: 10/13/2014
-- Description:   <Description,,>
-- =============================================
--usp_ACT_GetBillingFlowChartDetailsWithoutIscis 50266, 396
CREATE PROCEDURE [dbo].[usp_ACT_GetBillingFlowChartDetailsWithoutIscis]
	@proposal_id INT,
	@media_month_id INT
AS
BEGIN
	SELECT 
		n.code,
		pd.proposal_rate,
		pdw.*,
		sl.length
	FROM 
		dbo.proposal_details pd
		JOIN networks n (NOLOCK) ON n.id = pd.network_id
		JOIN dbo.proposal_detail_worksheets pdw (NOLOCK) ON pdw.proposal_detail_id = pd.id
		JOIN dbo.spot_lengths sl (NOLOCK) ON sl.id = pd.spot_length_id
		JOIN media_weeks mw (NOLOCK) ON mw.id=pdw.media_week_id
		JOIN dbo.media_months mm (NOLOCK) ON mm.id = mw.media_month_id
	WHERE 
		proposal_id = @proposal_id
		AND mm.id = @media_month_id
	ORDER BY
		n.code


	SELECT DISTINCT 
		mw.id,
		mw.week_number,
		mw.media_month_id,
		mm.year,
		mm.month,
		mw.start_date,
		mw.end_date,
		mm.start_date,
		mm.end_date
	FROM
		proposal_detail_worksheets pdw
		JOIN media_weeks mw (NOLOCK) ON mw.id=pdw.media_week_id
		JOIN proposal_details pd (NOLOCK) ON pd.id = pdw.proposal_detail_id
		JOIN dbo.media_months mm (NOLOCK) ON mm.id = mw.media_month_id
		
	WHERE 
		pd.proposal_id = @proposal_id
		AND mm.id = @media_month_id
	ORDER BY
		mw.start_date
	
END


