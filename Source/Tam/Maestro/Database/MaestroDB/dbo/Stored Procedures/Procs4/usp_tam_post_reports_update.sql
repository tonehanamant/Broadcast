CREATE PROCEDURE usp_tam_post_reports_update
(
	@id		Int,
	@tam_post_id		Int,
	@document_id		Int,
	@employee_id		Int,
	@date_created		DateTime
)
AS
UPDATE tam_post_reports SET
	tam_post_id = @tam_post_id,
	document_id = @document_id,
	employee_id = @employee_id,
	date_created = @date_created
WHERE
	id = @id

