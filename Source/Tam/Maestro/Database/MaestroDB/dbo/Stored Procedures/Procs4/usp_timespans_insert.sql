CREATE PROCEDURE usp_timespans_insert
(
	@id		Int		OUTPUT,
	@start_time		Int,
	@end_time		Int
)
AS
INSERT INTO timespans
(
	start_time,
	end_time
)
VALUES
(
	@start_time,
	@end_time
)

SELECT
	@id = SCOPE_IDENTITY()

