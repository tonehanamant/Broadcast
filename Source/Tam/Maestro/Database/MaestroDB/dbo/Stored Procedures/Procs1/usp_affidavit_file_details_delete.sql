CREATE PROCEDURE usp_affidavit_file_details_delete
(
	@id Int
)
AS
DELETE FROM affidavit_file_details WHERE id=@id
