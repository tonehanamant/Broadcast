CREATE PROCEDURE usp_businesses_update
(
	@id		Int,
	@code		VarChar(15),
	@name		VarChar(63),
	@type		VarChar(15),
	@active		Bit,
	@effective_date		DateTime
)
AS
UPDATE businesses SET
	code = @code,
	name = @name,
	type = @type,
	active = @active,
	effective_date = @effective_date
WHERE
	id = @id

