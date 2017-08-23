CREATE PROCEDURE usp_dayparts_update
(
	@id		Int,
	@timespan_id		Int,
	@code		VarChar(15),
	@name		VarChar(63),
	@tier		Int,
	@daypart_text		VarChar(63),
	@total_hours		Float
)
AS
UPDATE dayparts SET
	timespan_id = @timespan_id,
	code = @code,
	name = @name,
	tier = @tier,
	daypart_text = @daypart_text,
	total_hours = @total_hours
WHERE
	id = @id

