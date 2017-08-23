-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectZoneNetworkBusinessObjectsByZone]
	@zone_id int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		zn.*,
		n.code,
		z.code,
		z.name
	FROM
		zone_networks zn (NOLOCK)
		JOIN zones z (NOLOCK) ON z.id=zn.zone_id
		JOIN networks n (NOLOCK) ON n.id=zn.network_id
	WHERE
		zn.zone_id=@zone_id
END
