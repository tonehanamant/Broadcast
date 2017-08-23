CREATE PROCEDURE usp_daypart_days_insert
(
	@daypart_id		Int,
	@day_id		Int
)
AS
INSERT INTO daypart_days
(
	daypart_id,
	day_id
)
VALUES
(
	@daypart_id,
	@day_id
)

