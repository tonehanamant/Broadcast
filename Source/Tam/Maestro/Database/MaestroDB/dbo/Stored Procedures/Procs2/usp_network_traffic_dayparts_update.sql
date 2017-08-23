CREATE PROCEDURE usp_network_traffic_dayparts_update
(
	@nielsen_network_id		Int,
	@daypart_id		Int,
	@effective_date		DateTime
)
AS
UPDATE network_traffic_dayparts SET
	effective_date = @effective_date
WHERE
	nielsen_network_id = @nielsen_network_id AND
	daypart_id = @daypart_id
