-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/1/2012
-- Description:	
-- =============================================
-- usp_PCS_GetProposalWorksheetReport 21373,41
CREATE PROCEDURE usp_PCS_GetProposalWorksheetReport
	@proposal_id INT,
	@audience_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		pdw.proposal_detail_id,
		n.code,
		pdw.media_week_id,
		pd.proposal_rate 'cost',
		pdw.units 'units',
		CASE p.is_equivalized
			WHEN 1 THEN
				(pda.us_universe * pd.universal_scaling_factor * pda.rating) * sl.delivery_multiplier
			ELSE
				(pda.us_universe * pd.universal_scaling_factor * pda.rating)
		END 'delivery'
	FROM
		proposals p (NOLOCK)
		JOIN proposal_details pd (NOLOCK) ON pd.proposal_id=p.id
		JOIN proposal_detail_worksheets pdw (NOLOCK) ON pdw.proposal_detail_id=pd.id
		JOIN proposal_detail_audiences pda (NOLOCK) ON pda.proposal_detail_id=pd.id AND pda.audience_id=@audience_id
		JOIN networks n (NOLOCK) ON n.id=pd.network_id
		JOIN spot_lengths sl (NOLOCK) ON sl.id=pd.spot_length_id
	WHERE
		p.id=@proposal_id
		AND pd.include=1
	ORDER BY
		n.code
		
		
	SELECT DISTINCT
		mw.*
	FROM
		proposal_details pd (NOLOCK)
		JOIN proposal_detail_worksheets pdw (NOLOCK) ON pdw.proposal_detail_id=pd.id
		JOIN media_weeks mw (NOLOCK) ON mw.id=pdw.media_week_id
	WHERE
		pd.proposal_id=@proposal_id
	ORDER BY
		mw.start_date
		
		
	SELECT DISTINCT
		mm.*
	FROM
		proposal_details pd (NOLOCK)
		JOIN proposal_detail_worksheets pdw (NOLOCK) ON pdw.proposal_detail_id=pd.id
		JOIN media_weeks mw (NOLOCK) ON mw.id=pdw.media_week_id
		JOIN media_months mm (NOLOCK) ON mm.id=mw.media_month_id
	WHERE
		pd.proposal_id=@proposal_id		
	ORDER BY
		mm.start_date

	SET NOCOUNT OFF;
END
