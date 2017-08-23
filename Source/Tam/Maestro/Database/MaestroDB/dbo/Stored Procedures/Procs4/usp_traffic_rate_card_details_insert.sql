
CREATE PROCEDURE [dbo].[usp_traffic_rate_card_details_insert]
(
	@id		Int		OUTPUT,
	@traffic_rate_card_id		Int,
	@network_id		Int,
	@daypart_id		Int,
	@cluster_id		Int
)
AS
BEGIN
INSERT INTO traffic_rate_card_details
(
	traffic_rate_card_id,
	network_id,
	daypart_id,
	cluster_id
)
VALUES
(
	@traffic_rate_card_id,
	@network_id,
	@daypart_id,
	@cluster_id
)

SELECT
	@id = SCOPE_IDENTITY()
END

