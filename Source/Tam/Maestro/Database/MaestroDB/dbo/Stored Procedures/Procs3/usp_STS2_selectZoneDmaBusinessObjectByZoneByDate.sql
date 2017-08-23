-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectZoneDmaBusinessObjectByZoneByDate]
	@zone_id int,
	@effective_date datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		uvw_zonedma_universe.zone_id,
		uvw_zonedma_universe.dma_id,
		uvw_zonedma_universe.start_date,
		uvw_zonedma_universe.weight,
		uvw_zonedma_universe.end_date,
		uvw_dma_universe.name,
		uvw_zone_universe.code,
		uvw_zone_universe.name
	FROM
		uvw_zonedma_universe
		JOIN uvw_zone_universe ON uvw_zone_universe.zone_id=uvw_zonedma_universe.zone_id	AND (uvw_zone_universe.start_date<=@effective_date AND (uvw_zone_universe.end_date>=@effective_date OR uvw_zone_universe.end_date IS NULL))
		JOIN uvw_dma_universe ON uvw_dma_universe.dma_id=uvw_zonedma_universe.dma_id		AND (uvw_dma_universe.start_date<=@effective_date AND (uvw_dma_universe.end_date>=@effective_date OR uvw_dma_universe.end_date IS NULL))
	WHERE
		uvw_zonedma_universe.zone_id=@zone_id
		AND (uvw_zonedma_universe.start_date<=@effective_date AND (uvw_zonedma_universe.end_date>=@effective_date OR uvw_zonedma_universe.end_date IS NULL))
END
