-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectBillingSystemZoneBusinessObjectsBySystemByDate]
	@system_id int,
	@effective_date datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		uvw_systemzone_universe.zone_id,
		uvw_systemzone_universe.system_id,
		uvw_systemzone_universe.start_date,
		uvw_systemzone_universe.type,
		uvw_systemzone_universe.end_date,
		uvw_zone_universe.code,
		uvw_zone_universe.name,
		uvw_system_universe.code,
		uvw_system_universe.name
	FROM
		uvw_systemzone_universe
		JOIN uvw_zone_universe ON uvw_zone_universe.zone_id=uvw_systemzone_universe.zone_id			AND (uvw_zone_universe.start_date<=@effective_date AND (uvw_zone_universe.end_date>=@effective_date OR uvw_zone_universe.end_date IS NULL))
		JOIN uvw_system_universe ON uvw_system_universe.system_id=uvw_systemzone_universe.system_id	AND (uvw_system_universe.start_date<=@effective_date AND (uvw_system_universe.end_date>=@effective_date OR uvw_system_universe.end_date IS NULL))
	WHERE
		uvw_systemzone_universe.system_id=@system_id
		AND uvw_systemzone_universe.type='BILLING'
		AND (uvw_systemzone_universe.start_date<=@effective_date AND (uvw_systemzone_universe.end_date>=@effective_date OR uvw_systemzone_universe.end_date IS NULL))
END
