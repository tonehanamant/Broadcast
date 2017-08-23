CREATE PROCEDURE usp_notes_insert
(
	@id		Int		OUTPUT,
	@note_type		VarChar(63),
	@reference_id		Int,
	@employee_id		Int,
	@comment		Text,
	@date_created		DateTime,
	@date_last_modified		DateTime
)
AS
INSERT INTO notes
(
	note_type,
	reference_id,
	employee_id,
	comment,
	date_created,
	date_last_modified
)
VALUES
(
	@note_type,
	@reference_id,
	@employee_id,
	@comment,
	@date_created,
	@date_last_modified
)

SELECT
	@id = SCOPE_IDENTITY()

