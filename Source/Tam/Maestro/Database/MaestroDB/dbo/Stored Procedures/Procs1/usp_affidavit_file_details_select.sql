CREATE PROCEDURE usp_affidavit_file_details_select
(
	@id Int
)
AS
SELECT
	*
FROM
	affidavit_file_details WITH(NOLOCK)
WHERE
	id = @id
