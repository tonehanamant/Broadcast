
CREATE PROCEDURE [dbo].[usp_PCS_GetMediaPlanHistoryByAdvertiserAndProduct]
	@advertiser_company_id INT,
	@product_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT 
		p.id 'proposal_id',
		n.code 'network',
		pd.proposal_rate,
		CAST(pd.proposal_rate / (((pda.us_universe * pd.universal_scaling_factor * pda.rating) / 1000.0) * sl.delivery_multiplier) AS MONEY) 'hh_cpm',
		p.start_date,
		psl.length
	FROM
		proposal_detail_audiences pda	(NOLOCK) 
		JOIN proposal_details pd		(NOLOCK) ON pd.id=pda.proposal_detail_id
		JOIN proposals p				(NOLOCK) ON p.id=pd.proposal_id
		JOIN uvw_network_universe n		(NOLOCK) ON n.network_id=pd.network_id 
			AND (n.start_date<=p.start_date AND (n.end_date>=p.start_date OR n.end_date IS NULL))
		JOIN spot_lengths sl			(NOLOCK) ON sl.id=pd.spot_length_id
		JOIN spot_lengths psl			(NOLOCK) ON psl.id=p.default_spot_length_id
	WHERE
		p.advertiser_company_id=@advertiser_company_id
		AND p.proposal_status_id IN (3,4)
		AND p.product_id=@product_id
		AND pda.audience_id=31
		AND pd.num_spots<>0
	ORDER BY 
		p.id DESC,
		n.code ASC
END
