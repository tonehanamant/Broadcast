CREATE PROCEDURE usp_tam_post_reports_delete
(
	@id Int
)
AS
DELETE FROM tam_post_reports WHERE id=@id
