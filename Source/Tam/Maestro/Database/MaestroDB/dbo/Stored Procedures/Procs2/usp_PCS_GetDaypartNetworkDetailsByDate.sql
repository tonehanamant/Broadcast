
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/19/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetDaypartNetworkDetailsByDate]
	@effective_date DATETIME
AS
BEGIN
	SET NOCOUNT ON;

    SELECT
		nm.network_id 'daypart_network_id',
		CAST(nm.map_value AS INT) 'network_id',
		d.id,
		d.code,
		d.name,
		d.start_time,
		d.end_time,
		d.mon,
		d.tue,
		d.wed,
		d.thu,
		d.fri,
		d.sat,
		d.sun
	FROM 
		uvw_network_maps nm								(NOLOCK)
		JOIN uvw_network_maps nm_nielsen				(NOLOCK) ON nm_nielsen.network_id=nm.network_id AND nm_nielsen.map_set='Nielsen'
																	AND nm_nielsen.start_date<=@effective_date AND (nm_nielsen.end_date>=@effective_date OR nm_nielsen.end_date IS NULL)
		JOIN nielsen_networks nn						(NOLOCK) ON nn.nielsen_id=CAST(nm_nielsen.map_value AS INT)
		JOIN uvw_nielsen_network_rating_dayparts nnrd	(NOLOCK) ON nnrd.nielsen_network_id=nn.id
																	AND nnrd.start_date<=@effective_date AND (nnrd.end_date>=@effective_date OR nnrd.end_date IS NULL)
		JOIN vw_ccc_daypart d							(NOLOCK) ON d.id=nnrd.daypart_id
	WHERE 
		nm.map_set='DaypartNetworks' 
		AND nm.start_date<=@effective_date AND (nm.end_date>=@effective_date OR nm.end_date IS NULL)
END

