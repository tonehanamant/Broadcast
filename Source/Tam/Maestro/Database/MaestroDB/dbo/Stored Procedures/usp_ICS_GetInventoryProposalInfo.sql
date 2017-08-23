
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/9/2015
-- Description:	
-- =============================================
/*
	EXEC usp_ICS_GetInventoryProposalInfo '62931'
*/
CREATE PROCEDURE [dbo].[usp_ICS_GetInventoryProposalInfo]
	@proposal_ids NVARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
	
	DECLARE @proposal_ids2 TABLE (id INT NOT NULL, start_date DATETIME NOT NULL, PRIMARY KEY CLUSTERED(id, start_date))
	INSERT into @proposal_ids2
		SELECT s.id, p.start_date FROM dbo.SplitIntegers(@proposal_ids) s
		JOIN proposals p on p.id = s.id

	SELECT
		dp.*
	FROM
		uvw_display_proposals dp
	WHERE
		dp.id in (select id from @proposal_ids2)
	OPTION (RECOMPILE)
		
	SELECT
		pd.id 'proposal_detail_id',
		pd.network_id,
		n.code 'network',
		pd.num_spots 'units',
		pd.proposal_rate 'unit_rate',
		CASE 
			WHEN (pda.us_universe * pd.universal_scaling_factor * pda.rating) > 0.0 THEN
				ROUND(CAST(pd.proposal_rate / (((pda.us_universe * pd.universal_scaling_factor * pda.rating) / 1000.0) * sl.delivery_multiplier) AS MONEY), 2)
			ELSE
				0.0
		END 'eq_hh_cpm',
		CASE 
			WHEN (pda_dm.us_universe * pd.universal_scaling_factor * pda_dm.rating) > 0.0 THEN
				ROUND(CAST(pd.proposal_rate / (((pda_dm.us_universe * pd.universal_scaling_factor * pda_dm.rating) / 1000.0) * sl.delivery_multiplier) AS MONEY), 2)
			ELSE
				0.0
		END 'eq_demo_cpm',
		CAST(pd.num_spots * pd.topography_universe AS BIGINT) 'contracted_subscribers',
		((pda.us_universe * pd.universal_scaling_factor * pda.rating) / 1000.0) * sl.delivery_multiplier 'hh_eq_impressions_per_unit',
		((pda_dm.us_universe * pd.universal_scaling_factor * pda_dm.rating) / 1000.0) * sl.delivery_multiplier 'demo_eq_impressions_per_unit',
		pd.daypart_id,
		pd.topography_universe 'hh_coverage_universe',
		pd.proposal_id
	FROM
		proposal_details pd
		JOIN @proposal_ids2 p on pd.proposal_id = p.id
		JOIN proposal_detail_audiences pda ON pda.proposal_detail_id=pd.id
			AND pda.audience_id=31
		LEFT JOIN proposal_audiences pa ON pa.proposal_id=pd.proposal_id
			AND pa.ordinal=1
		LEFT JOIN proposal_detail_audiences pda_dm ON pda_dm.proposal_detail_id=pd.id
			AND pda_dm.audience_id=pa.audience_id
		JOIN spot_lengths sl (NOLOCK) ON sl.id=pd.spot_length_id
		JOIN uvw_network_universe n ON n.network_id=pd.network_id
			AND n.start_date<=p.start_date AND (n.end_date>=p.start_date OR n.end_date IS NULL)
	ORDER BY
		pd.id

	SELECT
		pdw.proposal_detail_id,
		mw.media_month_id,
		pdw.media_week_id,
		pdw.units,
		pd.proposal_id
	FROM
		proposal_details pd
		JOIN proposal_detail_worksheets pdw ON pdw.proposal_detail_id=pd.id
		JOIN media_weeks mw ON mw.id=pdw.media_week_id
	WHERE
		pdw.units > 0
		AND pd.proposal_id in (select id from @proposal_ids2)
	ORDER BY
		pdw.proposal_detail_id,
		mw.media_month_id,
		pdw.media_week_id
	OPTION (RECOMPILE)

	SELECT
		mw.id,
        pf.proposal_id,
        pf.start_date,
        pf.end_date,
        pf.selected
	FROM
		proposal_flights pf
		JOIN media_weeks mw ON (mw.start_date <= pf.start_date AND pf.start_date <= mw.end_date)
	WHERE
		pf.proposal_id in (select id from @proposal_ids2)
		AND
		pf.selected = 1
END