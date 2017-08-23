CREATE PROCEDURE usp_tam_post_excluded_affidavits_delete
(
	@id Int
)
AS
DELETE FROM tam_post_excluded_affidavits WHERE id=@id
