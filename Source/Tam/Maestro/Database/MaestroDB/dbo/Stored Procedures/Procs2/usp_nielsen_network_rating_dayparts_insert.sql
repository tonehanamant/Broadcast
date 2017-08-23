CREATE PROCEDURE usp_nielsen_network_rating_dayparts_insert
(
	@nielsen_network_id		Int,
	@daypart_id		Int,
	@effective_date		DateTime
)
AS
INSERT INTO nielsen_network_rating_dayparts
(
	nielsen_network_id,
	daypart_id,
	effective_date
)
VALUES
(
	@nielsen_network_id,
	@daypart_id,
	@effective_date
)

