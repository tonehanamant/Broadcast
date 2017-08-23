-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectZoneZoneBusinessObjectsByZone]
	@zone_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		zone_zones.primary_zone_id,
		zone_zones.secondary_zone_id,
		zone_zones.type,
		zone_zones.effective_date,
		zones.code,
		zones.name
	FROM
		zone_zones
		JOIN zones ON zones.id=zone_zones.secondary_zone_id
	WHERE
		zone_zones.primary_zone_id=@zone_id
END
