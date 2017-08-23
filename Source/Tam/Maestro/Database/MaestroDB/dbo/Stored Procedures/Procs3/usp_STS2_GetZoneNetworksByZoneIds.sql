-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_GetZoneNetworksByZoneIds]
	@zone_ids VARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		zn.*
	FROM
		zone_networks zn (NOLOCK)
	WHERE
		zn.zone_id IN (
			SELECT id FROM dbo.SplitIntegers(@zone_ids)
		)
END
