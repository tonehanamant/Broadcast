CREATE PROCEDURE usp_tam_post_excluded_affidavits_select
(
	@id Int
)
AS
SELECT
	*
FROM
	tam_post_excluded_affidavits WITH(NOLOCK)
WHERE
	id = @id
