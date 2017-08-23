
/***************************************************************************************************
** Date         Author          Description   
** ---------    ----------		-----------------------------------------------------
** XXXX			XXX
** 05/11/2017	Abdul Sukkur	Added column custom_traffic_system to systems 
*****************************************************************************************************/
CREATE PROCEDURE [dbo].[usp_STS2_selectTrafficSystemZoneBusinessObjectsByZone]
	@zone_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		system_zones.zone_id,
		system_zones.system_id,
		system_zones.type,
		system_zones.effective_date,
		zones.code,
		zones.name,
		systems.code,
		systems.name,
		systems.custom_traffic_system
	FROM
		system_zones
		JOIN zones ON zones.id=system_zones.zone_id
		JOIN systems ON systems.id=system_zones.system_id
	WHERE
		system_zones.zone_id=@zone_id
		AND system_zones.type='TRAFFIC'
END
