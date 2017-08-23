-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectZoneNetworkBusinessObjectsByZoneByDate]
	@zone_id int,
	@effective_date datetime
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		zn.zone_id,
		zn.network_id,
		zn.start_date,
		zn.source,
		zn.trafficable,
		zn.[primary],
		zn.subscribers,
		case when zn.end_date is null then '9999-12-31 23:59:59.997' else zn.end_date end,
		zn.feed_type,
		n.code,
		z.code,
		z.name
	FROM
		uvw_zonenetwork_universe zn
		JOIN uvw_zone_universe z ON z.zone_id=zn.zone_id			AND (z.start_date<=@effective_date AND (z.end_date>=@effective_date OR z.end_date IS NULL))
		JOIN uvw_network_universe n ON n.network_id=zn.network_id	AND (n.start_date<=@effective_date AND (n.end_date>=@effective_date OR n.end_date IS NULL))
	WHERE
		zn.zone_id=@zone_id
		AND (zn.start_date<=@effective_date AND (zn.end_date>=@effective_date OR zn.end_date IS NULL))
END
