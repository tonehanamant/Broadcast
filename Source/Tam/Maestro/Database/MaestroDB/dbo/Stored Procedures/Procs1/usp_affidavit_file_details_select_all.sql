CREATE PROCEDURE usp_affidavit_file_details_select_all
AS
SELECT
	*
FROM
	affidavit_file_details WITH(NOLOCK)
