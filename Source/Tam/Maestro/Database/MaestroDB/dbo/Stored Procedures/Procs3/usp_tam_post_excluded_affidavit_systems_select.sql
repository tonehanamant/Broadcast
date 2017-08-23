CREATE PROCEDURE usp_tam_post_excluded_affidavit_systems_select
(
	@tam_post_excluded_affidavit_id		Int,
	@system_id		Int
)
AS
SELECT
	*
FROM
	tam_post_excluded_affidavit_systems WITH(NOLOCK)
WHERE
	tam_post_excluded_affidavit_id=@tam_post_excluded_affidavit_id
	AND
	system_id=@system_id

