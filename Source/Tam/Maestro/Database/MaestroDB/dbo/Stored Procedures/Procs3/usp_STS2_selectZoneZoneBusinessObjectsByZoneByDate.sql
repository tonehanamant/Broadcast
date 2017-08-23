-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectZoneZoneBusinessObjectsByZoneByDate]
	@zone_id int,
	@effective_date datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		uvw_zonezone_universe.primary_zone_id,
		uvw_zonezone_universe.secondary_zone_id,
		uvw_zonezone_universe.type,
		uvw_zonezone_universe.start_date,
		uvw_zonezone_universe.end_date,
		uvw_zone_universe.code,
		uvw_zone_universe.name
	FROM
		uvw_zonezone_universe
		JOIN uvw_zone_universe ON uvw_zone_universe.zone_id=uvw_zonezone_universe.secondary_zone_id AND (uvw_zone_universe.start_date<=@effective_date AND (uvw_zone_universe.end_date>=@effective_date OR uvw_zone_universe.end_date IS NULL))
	WHERE
		uvw_zonezone_universe.primary_zone_id=@zone_id
		AND (uvw_zonezone_universe.start_date<=@effective_date AND (uvw_zonezone_universe.end_date>=@effective_date OR uvw_zonezone_universe.end_date IS NULL))
END
