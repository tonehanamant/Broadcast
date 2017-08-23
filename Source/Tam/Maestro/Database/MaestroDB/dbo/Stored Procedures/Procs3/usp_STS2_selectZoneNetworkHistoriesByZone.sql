CREATE PROCEDURE [dbo].[usp_STS2_selectZoneNetworkHistoriesByZone]
	@zone_id int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		znh.*
	FROM
		zone_network_histories znh (NOLOCK)
	WHERE
		znh.zone_id=@zone_id
	ORDER BY
		znh.network_id,
		znh.start_date DESC
END
