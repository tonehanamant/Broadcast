CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficDetailsWithRatesByTrafficID]
	@traffic_id INT
AS
BEGIN
	SELECT 
		td.id, 
		td.network_id, 
		td.daypart_id, 
		SUM(tdpdm.proposal_rate)
	FROM
		traffic_details td (NOLOCK) 
		join traffic_details_proposal_details_map tdpdm (NOLOCK) ON tdpdm.traffic_detail_id = td.id
	WHERE 
		td.traffic_id = @traffic_id
	GROUP BY 
		td.id, 
		td.network_id, 
		td.daypart_id
	ORDER BY 
		td.network_id
END
