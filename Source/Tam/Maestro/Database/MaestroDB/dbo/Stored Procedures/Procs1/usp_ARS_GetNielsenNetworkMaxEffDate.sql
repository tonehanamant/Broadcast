

CREATE proc [dbo].[usp_ARS_GetNielsenNetworkMaxEffDate]
(
	@nielsen_network_id INT
)
AS
BEGIN
	SET NOCOUNT ON;

	WITH EffDates AS (
		SELECT
			effective_date
		FROM
			nielsen_networks WITH(NOLOCK)
		WHERE
			id=@nielsen_network_id
	UNION
		SELECT
			effective_date
		FROM
			network_breaks WITH(NOLOCK)
		WHERE
			nielsen_network_id = @nielsen_network_id
	UNION
		SELECT
			nielsen_network_rating_dayparts.effective_date
		FROM
			nielsen_network_rating_dayparts	WITH(NOLOCK)
		WHERE
			nielsen_network_rating_dayparts.nielsen_network_id=@nielsen_network_id
	UNION
		SELECT
			network_traffic_dayparts.effective_date
		FROM
			network_traffic_dayparts WITH(NOLOCK)
		WHERE
			network_traffic_dayparts.nielsen_network_id=@nielsen_network_id
	)
	SELECT 
		max(effective_date) as max_effective_date
	FROM
		EffDates;
	

END
