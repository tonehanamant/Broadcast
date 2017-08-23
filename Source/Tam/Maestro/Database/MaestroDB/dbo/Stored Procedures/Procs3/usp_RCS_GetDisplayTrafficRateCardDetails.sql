
CREATE PROCEDURE [dbo].[usp_RCS_GetDisplayTrafficRateCardDetails]
	@traffic_rate_card_id INT
AS
BEGIN
	SET NOCOUNT ON;

	-- traffic_rate_card_details
    SELECT
		trcd.id,
		dayparts.code + ' ' + dayparts.daypart_text,
		n.code,
		trcd.network_id,
		c.name,
		trcd.cluster_id,
		c.cluster_type
	FROM
		traffic_rate_card_details trcd (NOLOCK)
		left JOIN networks n (NOLOCK) ON n.id=trcd.network_id
		left JOIN clusters c (NOLOCK) ON c.id=trcd.cluster_id
		JOIN traffic_rate_cards trc (NOLOCK) ON trc.id=trcd.traffic_rate_card_id
		JOIN dayparts (NOLOCK) ON dayparts.id=trcd.daypart_id
	WHERE
		trc.id=@traffic_rate_card_id
	ORDER BY
		CASE WHEN n.code IS NULL THEN 2 ELSE 1 END, n.code
	
	-- traffic_rate_card_detail_rates
	SELECT
		trcdr.traffic_rate_card_detail_id,
		sl.length,
		trcdr.rate
	FROM
		traffic_rate_card_detail_rates trcdr (NOLOCK)
		JOIN spot_lengths sl (NOLOCK) ON sl.id=trcdr.spot_length_id
	WHERE
		traffic_rate_card_detail_id IN (
			SELECT
				id
			FROM
				traffic_rate_card_details (NOLOCK)
			WHERE 
				traffic_rate_card_id=@traffic_rate_card_id
			)
END
