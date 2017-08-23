CREATE PROCEDURE usp_traffic_detail_weeks_select
(
	@id Int
)
AS
SELECT
	id,
	traffic_detail_id,
	start_date,
	end_date,
	suspended
FROM
	traffic_detail_weeks (NOLOCK)
WHERE
	id = @id
