CREATE PROCEDURE [dbo].[usp_STS2_selectZoneNetworkHistoriesByZoneAndNetwork]
	@zone_id int,
	@network_id int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		znh.*
	FROM
		zone_network_histories znh (NOLOCK)
	WHERE
		znh.zone_id=@zone_id
		AND znh.network_id=@network_id
	ORDER BY
		znh.start_date DESC
END
