-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_BRS_GetNetworkIDFromSystemID]
(
@systemID int,
@networkID int
)
AS
BEGIN

	SET NOCOUNT ON;

	SELECT TOP 1
		network_id,zone_id,subscribers
	FROM
		zone_networks zn (nolock)
	WHERE
		zone_id in 
				(SELECT
					zone_id
				FROM
					system_zones sz (nolock)
				WHERE
					system_id = @systemID
				AND
					[type] = 'BILLING')
		AND
			network_id = @networkID
END
