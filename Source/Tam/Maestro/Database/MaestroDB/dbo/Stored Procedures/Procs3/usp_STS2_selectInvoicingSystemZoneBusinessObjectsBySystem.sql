-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectInvoicingSystemZoneBusinessObjectsBySystem]
	@system_id int
AS
BEGIN
	SET NOCOUNT ON;

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
		system_zones.system_id=@system_id
		AND system_zones.type='INVOICING'
END
