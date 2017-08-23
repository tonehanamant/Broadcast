CREATE PROCEDURE usp_tam_post_excluded_affidavit_systems_select_all
AS
SELECT
	*
FROM
	tam_post_excluded_affidavit_systems WITH(NOLOCK)
