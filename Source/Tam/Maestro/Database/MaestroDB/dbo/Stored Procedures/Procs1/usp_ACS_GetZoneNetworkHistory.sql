-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
-- usp_ACS_GetZoneNetworkHistory '3/28/2011','4/24/2011'
CREATE PROCEDURE [dbo].[usp_ACS_GetZoneNetworkHistory] 
	@min_date DATETIME,
	@max_date DATETIME
AS
BEGIN
	SET NOCOUNT ON;

    SELECT
		zone_id,
		network_id,
		subscribers,
		start_date,
		end_date,
		feed_type
	FROM
		uvw_zonenetwork_universe (NOLOCK)
	WHERE
		(end_date IS NULL OR (start_date <= @max_date AND end_date >= @min_date))
END
