-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectManagedByZoneBusinessBusinessObjectByZoneByDate]
	@zone_id int,
	@effective_date datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		uvw_zonebusiness_universe.zone_id,
		uvw_zonebusiness_universe.business_id,
		uvw_zonebusiness_universe.start_date,
		uvw_zonebusiness_universe.type,
		uvw_zonebusiness_universe.end_date,
		uvw_business_universe.name,
		uvw_zone_universe.code,
		uvw_zone_universe.name
	FROM
		uvw_zonebusiness_universe
		JOIN uvw_zone_universe ON uvw_zone_universe.zone_id=uvw_zonebusiness_universe.zone_id					AND (uvw_zone_universe.start_date<=@effective_date AND (uvw_zone_universe.end_date>=@effective_date OR uvw_zone_universe.end_date IS NULL))
		JOIN uvw_business_universe ON uvw_business_universe.business_id=uvw_zonebusiness_universe.business_id	AND (uvw_business_universe.start_date<=@effective_date AND (uvw_business_universe.end_date>=@effective_date OR uvw_business_universe.end_date IS NULL))
	WHERE
		uvw_zonebusiness_universe.zone_id=@zone_id
		AND uvw_zonebusiness_universe.type='MANAGEDBY'
		AND (uvw_zonebusiness_universe.start_date<=@effective_date AND (uvw_zonebusiness_universe.end_date>=@effective_date OR uvw_zonebusiness_universe.end_date IS NULL))
END
