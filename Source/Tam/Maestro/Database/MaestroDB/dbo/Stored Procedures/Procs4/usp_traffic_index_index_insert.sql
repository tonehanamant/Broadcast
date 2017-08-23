CREATE PROCEDURE usp_traffic_index_index_insert
(
	@id		Int		OUTPUT,
	@media_month_id		Int
)
AS
INSERT INTO traffic_index_index
(
	media_month_id
)
VALUES
(
	@media_month_id
)

SELECT
	@id = SCOPE_IDENTITY()

