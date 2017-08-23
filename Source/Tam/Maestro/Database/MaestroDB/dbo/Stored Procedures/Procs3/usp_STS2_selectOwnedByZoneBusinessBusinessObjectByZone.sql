-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectOwnedByZoneBusinessBusinessObjectByZone]
	@zone_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		zone_businesses.zone_id,
		zone_businesses.business_id,
		zone_businesses.type,
		zone_businesses.effective_date,
		businesses.name,
		zones.code,
		zones.name
	FROM
		zone_businesses
		JOIN zones ON zones.id=zone_businesses.zone_id
		JOIN businesses ON businesses.id=zone_businesses.business_id
	WHERE
		zone_businesses.zone_id=@zone_id
		AND zone_businesses.type='OWNEDBY'
END
