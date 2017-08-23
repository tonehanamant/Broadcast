-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/20/2011
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectInvoicingSystemZoneBusinessObjectsByZone]
	@zone_id int
AS
BEGIN
	SELECT
		system_zones.zone_id,
		system_zones.system_id,
		system_zones.type,
		system_zones.effective_date,
		zones.code,
		zones.name,
		systems.code,
		systems.name
	FROM
		system_zones
		JOIN zones ON zones.id=system_zones.zone_id
		JOIN systems ON systems.id=system_zones.system_id
	WHERE
		system_zones.zone_id=@zone_id
		AND system_zones.type='INVOICING'
END
