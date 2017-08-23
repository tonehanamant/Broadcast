
/***************************************************************************************************
** Date			Author			Description	
** ---------	----------		--------------------------------------------------------------------
** 08/20/2015	Abdul Sukkur	Get valid display dayparts
*****************************************************************************************************/
CREATE PROCEDURE [dbo].[usp_ARS_GetValidDisplayDaypartsForNetworkForTraffic]
	@network_id INT,
	@effective_date datetime 
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		id,
		code,
		name,
		start_time,
		end_time,
		mon,
		tue,
		wed,
		thu,
		fri,
		sat,
		sun
	FROM
		vw_ccc_daypart
	WHERE
		id IN (
			SELECT d.daypart_id FROM uvw_network_traffic_dayparts (NOLOCK) d WHERE 
				d.[start_date] <=@effective_date AND (d.[end_date] >=@effective_date OR d.[end_date] IS NULL)  AND	d.nielsen_network_id IN (
				SELECT n.nielsen_network_id FROM uvw_nielsen_network_universes n (NOLOCK) WHERE 
				n.[start_date] <=@effective_date AND (n.[end_date] >=@effective_date OR n.[end_date] IS NULL)  AND n.nielsen_id IN (
					SELECT CAST(m.map_value AS INT) FROM uvw_network_maps m (NOLOCK) WHERE 
					m.[start_date] <=@effective_date AND (m.[end_date] >=@effective_date OR m.[end_date] IS NULL)  AND m.map_set='Nielsen' AND m.network_id=@network_id
				)
			)
		)
END