-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_GetZoneAndZoneNetworks]
	@zone_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		z.*
	FROM
		zones z (NOLOCK)
	WHERE
		z.id=@zone_id

	SELECT
		zn.*
	FROM
		zone_networks zn (NOLOCK)
	WHERE
		zn.zone_id=@zone_id
END
