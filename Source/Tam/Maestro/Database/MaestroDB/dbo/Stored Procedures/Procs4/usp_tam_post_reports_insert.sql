CREATE PROCEDURE usp_tam_post_reports_insert
(
	@id		Int		OUTPUT,
	@tam_post_id		Int,
	@document_id		Int,
	@employee_id		Int,
	@date_created		DateTime
)
AS
INSERT INTO tam_post_reports
(
	tam_post_id,
	document_id,
	employee_id,
	date_created
)
VALUES
(
	@tam_post_id,
	@document_id,
	@employee_id,
	@date_created
)

SELECT
	@id = SCOPE_IDENTITY()

