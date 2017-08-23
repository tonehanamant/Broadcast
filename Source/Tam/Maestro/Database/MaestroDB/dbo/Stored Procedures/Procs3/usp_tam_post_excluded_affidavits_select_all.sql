CREATE PROCEDURE usp_tam_post_excluded_affidavits_select_all
AS
SELECT
	*
FROM
	tam_post_excluded_affidavits WITH(NOLOCK)
