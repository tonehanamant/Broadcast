CREATE PROCEDURE usp_traffic_index_index_update
(
	@id		Int,
	@media_month_id		Int
)
AS
UPDATE traffic_index_index SET
	media_month_id = @media_month_id
WHERE
	id = @id

