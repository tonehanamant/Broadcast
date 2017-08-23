
CREATE PROCEDURE [dbo].[usp_RCS_GetClusterRateCardBusinessObject] 
	@cluster_rate_card_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	-- cluster rate cards
	exec usp_cluster_rate_cards_select @cluster_rate_card_id

	-- rate card details
	SELECT
		c.name,
		trcd.id,
		trcd.cluster_rate_card_id,
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
		cluster_rate_card_details trcd (NOLOCK)
		JOIN cluster_rate_cards trc (NOLOCK) on trc.id = trcd.cluster_rate_card_id
		left join clusters c (nolock) on trcd.cluster_id = c.id
		JOIN vw_ccc_daypart d (NOLOCK) on d.id=trcd.daypart_id
	WHERE
		trc.id = @cluster_rate_card_id

	-- rate card detail rates
	SELECT
		*
	FROM
		cluster_rate_card_detail_rates (NOLOCK)
	WHERE
		cluster_rate_card_detail_id IN (
			SELECT
				id
			FROM
				cluster_rate_card_details (NOLOCK)
			WHERE 
				cluster_rate_card_id=@cluster_rate_card_id
		)

	SELECT
		*
	FROM
		cluster_rate_cards trc (NOLOCK)
		JOIN topographies t (NOLOCK) on t.id = trc.topography_id
	WHERE
		t.id = (SELECT topography_id FROM cluster_rate_cards (NOLOCK) WHERE id = @cluster_rate_card_id)
		AND trc.id <> @cluster_rate_card_id
		AND trc.end_date IS NULL


END