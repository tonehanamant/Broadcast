
CREATE PROCEDURE [dbo].[usp_RCS_GetTrafficRateCardBusinessObject] 
	@traffic_rate_card_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	-- traffic rate cards
	exec usp_traffic_rate_cards_select @traffic_rate_card_id

	-- rate card details
	SELECT
		n.code,
		trcd.id,
		trcd.traffic_rate_card_id,
		trcd.network_id,
		trcd.daypart_id,
		trcd.cluster_id,		
		d.id,
		d.code,
		d.[name],
		d.start_time,
		d.end_time,
		d.mon,
		d.tue,
		d.wed,
		d.thu,
		d.fri,
		d.sat,
		d.sun,
		c.name,		
		c.topography_id,
		c.cluster_type
	FROM
		traffic_rate_card_details trcd (NOLOCK)
		JOIN traffic_rate_cards trc (NOLOCK) on trc.id = trcd.traffic_rate_card_id
		left JOIN uvw_network_universe n	(NOLOCK) ON n.network_id=trcd.network_id AND (n.start_date<=trc.start_date AND (n.end_date>=trc.start_date OR n.end_date IS NULL))		
		left join clusters c (nolock) on trcd.cluster_id = c.id
		JOIN vw_ccc_daypart d (NOLOCK) on d.id=trcd.daypart_id
	WHERE
		trc.id = @traffic_rate_card_id

	-- rate card detail rates
	SELECT
		*
	FROM
		traffic_rate_card_detail_rates (NOLOCK)
	WHERE
		traffic_rate_card_detail_id IN (
			SELECT
				id
			FROM
				traffic_rate_card_details (NOLOCK)
			WHERE 
				traffic_rate_card_id=@traffic_rate_card_id
		)

	SELECT
		*
	FROM
		traffic_rate_cards trc (NOLOCK)
		JOIN topographies t (NOLOCK) on t.id = trc.topography_id
	WHERE
		t.id = (SELECT topography_id FROM traffic_rate_cards (NOLOCK) WHERE id = @traffic_rate_card_id)
		AND trc.id <> @traffic_rate_card_id
		AND trc.end_date IS NULL


END
