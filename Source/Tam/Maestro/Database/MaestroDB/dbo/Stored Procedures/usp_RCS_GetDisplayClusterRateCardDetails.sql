
CREATE PROCEDURE [dbo].[usp_RCS_GetDisplayClusterRateCardDetails]
	@cluster_rate_card_id INT
AS
BEGIN
	SET NOCOUNT ON;

	-- cluster_rate_card_details
    SELECT
		trcd.id,
		dayparts.code + ' ' + dayparts.daypart_text,
		null,
		null,
		c.name,
		trcd.cluster_id,
		c.cluster_type
	FROM
		cluster_rate_card_details trcd (NOLOCK)
		left JOIN clusters c (NOLOCK) ON c.id=trcd.cluster_id
		JOIN cluster_rate_cards trc (NOLOCK) ON trc.id=trcd.cluster_rate_card_id
		JOIN dayparts (NOLOCK) ON dayparts.id=trcd.daypart_id
	WHERE
		trc.id=@cluster_rate_card_id
	ORDER BY
		c.name
	
	-- cluster_rate_card_detail_rates
	SELECT
		trcdr.cluster_rate_card_detail_id,
		sl.length,
		trcdr.rate
	FROM
		cluster_rate_card_detail_rates trcdr (NOLOCK)
		JOIN spot_lengths sl (NOLOCK) ON sl.id=trcdr.spot_length_id
	WHERE
		cluster_rate_card_detail_id IN (
			SELECT
				id
			FROM
				cluster_rate_card_details (NOLOCK)
			WHERE 
				cluster_rate_card_id=@cluster_rate_card_id
			)
END