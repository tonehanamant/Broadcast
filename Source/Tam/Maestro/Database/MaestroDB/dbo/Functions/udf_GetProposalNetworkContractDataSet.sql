-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 2/21/2014
-- Description:	Returns contracted data at the proposal/network level for a given media plan and demographic
-- =============================================
-- SELECT * FROM dbo.udf_GetProposalNetworkContractDataSet(50610,37)
CREATE FUNCTION [dbo].[udf_GetProposalNetworkContractDataSet]
(	
	@proposal_id INT,
	@audience_id INT
)
RETURNS @return TABLE
(
	proposal_id INT, 
	network_id INT, 
	audience_id INT, 
	delivery FLOAT, 
	eq_delivery FLOAT, 
	total_cost MONEY, 
	rate MONEY, 
	universe FLOAT,
	num_spots INT,
	cpm MONEY, 
	eq_cpm MONEY,
	vpvh FLOAT
) 
AS
BEGIN
	INSERT INTO @return
		SELECT
			pd.proposal_id,
			pd.network_id,
			@audience_id,
			SUM(
				CAST(pd.num_spots AS FLOAT)
				*
				(pda.us_universe * pd.universal_scaling_factor * pda.rating)
			) 'delivery',
			SUM(
				CAST(pd.num_spots AS FLOAT)
				*
				((pda.us_universe * pd.universal_scaling_factor * pda.rating) * sl.delivery_multiplier)
			) 'eq_delivery',
			SUM(
				CAST(pd.num_spots AS MONEY)
				*
				pd.proposal_rate
			) 'total_cost',
			CASE 
				WHEN SUM(CAST(pd.num_spots AS MONEY)) > 0 THEN
					SUM(CAST(pd.num_spots AS MONEY) * pd.proposal_rate) / SUM(CAST(pd.num_spots AS MONEY))
				ELSE
					0
			END 'rate',
			CASE 
				WHEN SUM(CAST(pd.num_spots AS MONEY)) > 0 THEN
					SUM(CAST(pd.num_spots AS FLOAT) * pd.topography_universe) / CAST(SUM(pd.num_spots) AS FLOAT)
				ELSE
					AVG(pd.topography_universe)
			END 'universe', -- weighted average
			SUM(pd.num_spots) 'num_spots',
			NULL,
			NULL,
			CASE WHEN SUM(CAST(pd.num_spots AS FLOAT) * (pda_hh.us_universe * pd.universal_scaling_factor * pda_hh.rating)) > 0 THEN
				SUM(CAST(pd.num_spots AS FLOAT) * (pda.us_universe * pd.universal_scaling_factor * pda.rating))
				/
				SUM(CAST(pd.num_spots AS FLOAT) * (pda_hh.us_universe * pd.universal_scaling_factor * pda_hh.rating))
			ELSE
				0.0
			END 'vpvh'
		FROM
			proposal_details pd						(NOLOCK)
			JOIN proposal_detail_audiences pda		(NOLOCK) ON pda.proposal_detail_id=pd.id AND pda.audience_id=@audience_id
			JOIN proposal_detail_audiences pda_hh	(NOLOCK) ON pda_hh.proposal_detail_id=pd.id AND pda_hh.audience_id=31
			JOIN spot_lengths sl					(NOLOCK) ON sl.id=pd.spot_length_id
		WHERE
			pd.proposal_id=@proposal_id
		GROUP BY
			pd.proposal_id,
			pd.network_id;
	
	-- calculate cpm's
	UPDATE @return SET 
		cpm		= CASE WHEN delivery > 0	THEN total_cost / (delivery / 1000.0)		ELSE NULL END,
		eq_cpm  = CASE WHEN eq_delivery > 0 THEN total_cost / (eq_delivery / 1000.0)	ELSE NULL END;
		
	RETURN;
END
