-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/1/2013
-- Description:	This is a rewrite of usp_REL_GetProposalDetailsForSpotCost + usp_REL_GetTrafficUniverseForSpotCost for performance purposes.
-- =============================================
-- usp_REL_GetProposalDetailsForSpotCost2 48487,35769,NULL
CREATE PROCEDURE usp_REL_GetProposalDetailsForSpotCost2
	@proposal_id INT,
	@traffic_id INT,
	@audience_id INT -- optional
WITH RECOMPILE
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @proposal_id2 INT
	DECLARE @traffic_id2 INT
	DECLARE @audience_id2 INT
	
	SET @proposal_id2 = @proposal_id
	SET @traffic_id2 = @traffic_id
	SET @audience_id2 = @audience_id

	CREATE TABLE #topographies (topography_id INT)
	INSERT INTO #topographies
		SELECT DISTINCT
			tdt.topography_id 
		FROM 
			traffic_detail_topographies tdt (NOLOCK)
			JOIN traffic_detail_weeks tdw (NOLOCK) on tdw.id = tdt.traffic_detail_week_id
			JOIN traffic_details td (NOLOCK) on td.id = tdw.traffic_detail_id
		WHERE 
			td.traffic_id=@traffic_id2 
		ORDER BY 
			tdt.topography_id;

	-- (1) topographies
	SELECT * FROM #topographies;

	-- (2) proposal/traffic details
	CREATE TABLE #details (network_id INT, daypart_id INT, audience_id INT, cpm FLOAT, rating FLOAT, proposal_rate MONEY, proposal_hh_coverage_universe FLOAT, traffic_rating FLOAT, proposal_universe_for_cpm FLOAT, traffic_primary_universe FLOAT, delivery_multiplier FLOAT, traffic_detail_id INT)
	INSERT INTO #details
		SELECT 
			pd.network_id, 
			pd.daypart_id, 
			pda.audience_id,
			CASE
				WHEN 
					(pda.rating * pda.us_universe * pd.universal_scaling_factor) = 0 
				THEN 
					0
				ELSE 
					pd.proposal_rate / ( (pda.rating * pda.us_universe * pd.universal_scaling_factor) / 1000.0 )
			END [CPM],
			pda.rating, 
			pd.proposal_rate, 
			dbo.GetProposalDetailCoverageUniverse(pd.id, 31) [Proposal HH Universe],
			case when traffic_detail_audiences.traffic_rating is null then 0.0 else traffic_detail_audiences.traffic_rating end,
			dbo.GetProposalDetailCoverageUniverse(pd.id, pda.audience_id) [Proposal Universe For CPM],
			dbo.GetTrafficDetailCoverageUniverse(traffic_details.id, traffic_detail_audiences.audience_id, 1) AS Traf_Prim_Universe,
			spot_lengths.delivery_multiplier,
			traffic_details.id
		FROM 
			proposal_details pd (NOLOCK) 
			JOIN proposal_detail_audiences pda (NOLOCK) on pd.id = pda.proposal_detail_id 
			JOIN proposals p (NOLOCK) on p.id = pd.proposal_id 
			JOIN proposal_audiences pa (NOLOCK) on pa.audience_id = pda.audience_id 
				AND pa.proposal_id = p.id  
			JOIN traffic_details (NOLOCK) on traffic_details.network_id = pd.network_id
			JOIN spot_lengths (NOLOCK) on pd.spot_length_id = spot_lengths.id
			LEFT JOIN traffic_detail_audiences (NOLOCK) on traffic_detail_audiences.traffic_detail_id = traffic_details.id 
				AND traffic_detail_audiences.audience_id = pa.audience_id
		WHERE 
			pd.proposal_id = @proposal_id2 
			AND traffic_details.traffic_id = @traffic_id2 
			AND ((@audience_id2 IS NULL AND pa.ordinal = CASE WHEN p.guarantee_type = 0 THEN 0 ELSE 1 END) OR (@audience_id2 IS NOT NULL AND pa.audience_id=@audience_id2));

	SELECT * FROM #details;

	-- (3) traffic universe for spot cost
	CREATE TABLE #traffic_universes (traffic_detail_id INT, topography_id INT, audience_id INT)
	INSERT INTO #traffic_universes
		SELECT DISTINCT
			d.traffic_detail_id,
			t.topography_id,
			d.audience_id
		FROM
			#details d
			CROSS APPLY #topographies t
		ORDER BY
			d.traffic_detail_id,
			t.topography_id,
			d.audience_id;
			
	SELECT
		traffic_detail_id,
		topography_id,
		audience_id,
		dbo.GetTrafficDetailCoverageUniverse(traffic_detail_id, audience_id, topography_id) 'traffic_universe_for_spot_rate'
	FROM
		#traffic_universes;
		
		
	DROP TABLE #topographies;
	DROP TABLE #details;
	DROP TABLE #traffic_universes;
END
