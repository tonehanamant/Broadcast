-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectZoneNetworksByZone]
	@zone_id int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		zn.*
	FROM
		zone_networks zn (NOLOCK)
	WHERE
		zone_id=@zone_id
END
