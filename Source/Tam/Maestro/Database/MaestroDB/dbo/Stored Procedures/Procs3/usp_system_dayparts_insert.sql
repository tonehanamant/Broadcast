CREATE PROCEDURE usp_system_dayparts_insert
(
	@system_id		Int,
	@daypart_id		Int,
	@effective_date		DateTime
)
AS
INSERT INTO system_dayparts
(
	system_id,
	daypart_id,
	effective_date
)
VALUES
(
	@system_id,
	@daypart_id,
	@effective_date
)

