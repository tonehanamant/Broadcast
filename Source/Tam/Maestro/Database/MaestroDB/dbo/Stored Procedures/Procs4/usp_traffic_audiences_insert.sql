CREATE PROCEDURE usp_traffic_audiences_insert
(
	@traffic_id		Int,
	@audience_id		Int,
	@ordinal		TinyInt,
	@universe		Float
)
AS
INSERT INTO traffic_audiences
(
	traffic_id,
	audience_id,
	ordinal,
	universe
)
VALUES
(
	@traffic_id,
	@audience_id,
	@ordinal,
	@universe
)

