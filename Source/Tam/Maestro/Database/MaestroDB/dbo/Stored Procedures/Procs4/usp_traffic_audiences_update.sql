CREATE PROCEDURE usp_traffic_audiences_update
(
	@traffic_id		Int,
	@audience_id		Int,
	@ordinal		TinyInt,
	@universe		Float
)
AS
UPDATE traffic_audiences SET
	ordinal = @ordinal,
	universe = @universe
WHERE
	traffic_id = @traffic_id AND
	audience_id = @audience_id
