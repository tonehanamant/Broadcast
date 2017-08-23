CREATE PROCEDURE usp_dayparts_insert
(
	@id		Int		OUTPUT,
	@timespan_id		Int,
	@code		VarChar(15),
	@name		VarChar(63),
	@tier		Int,
	@daypart_text		VarChar(63),
	@total_hours		Float
)
AS
INSERT INTO dayparts
(
	timespan_id,
	code,
	name,
	tier,
	daypart_text,
	total_hours
)
VALUES
(
	@timespan_id,
	@code,
	@name,
	@tier,
	@daypart_text,
	@total_hours
)

SELECT
	@id = SCOPE_IDENTITY()

