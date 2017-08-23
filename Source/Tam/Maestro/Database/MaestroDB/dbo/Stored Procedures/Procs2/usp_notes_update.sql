CREATE PROCEDURE usp_notes_update
(
	@id		Int,
	@note_type		VarChar(63),
	@reference_id		Int,
	@employee_id		Int,
	@comment		Text,
	@date_created		DateTime,
	@date_last_modified		DateTime
)
AS
UPDATE notes SET
	note_type = @note_type,
	reference_id = @reference_id,
	employee_id = @employee_id,
	comment = @comment,
	date_created = @date_created,
	date_last_modified = @date_last_modified
WHERE
	id = @id

