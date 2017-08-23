-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectZoneStateBusinessObjectsByZoneByDate]
	@zone_id int,
	@effective_date datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		uvw_zonestate_universe.zone_id,
		uvw_zonestate_universe.state_id,
		uvw_zonestate_universe.start_date,
		uvw_zonestate_universe.weight,
		uvw_zonestate_universe.end_date,
		uvw_state_universe.code,
		uvw_state_universe.name,
		uvw_zone_universe.code,
		uvw_zone_universe.name
	FROM
		uvw_zonestate_universe
		JOIN uvw_zone_universe ON uvw_zone_universe.zone_id=uvw_zonestate_universe.zone_id		AND (uvw_zone_universe.start_date<=@effective_date AND (uvw_zone_universe.end_date>=@effective_date OR uvw_zone_universe.end_date IS NULL))
		JOIN uvw_state_universe ON uvw_state_universe.state_id=uvw_zonestate_universe.state_id	AND (uvw_state_universe.start_date<=@effective_date AND (uvw_state_universe.end_date>=@effective_date OR uvw_state_universe.end_date IS NULL))
	WHERE
		uvw_zonestate_universe.zone_id=@zone_id
		AND (uvw_zonestate_universe.start_date<=@effective_date AND (uvw_zonestate_universe.end_date>=@effective_date OR uvw_zonestate_universe.end_date IS NULL))
END
