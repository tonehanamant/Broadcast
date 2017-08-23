CREATE PROCEDURE usp_system_dayparts_update
(
	@system_id		Int,
	@daypart_id		Int,
	@effective_date		DateTime
)
AS
UPDATE system_dayparts SET
	effective_date = @effective_date
WHERE
	system_id = @system_id AND
	daypart_id = @daypart_id
