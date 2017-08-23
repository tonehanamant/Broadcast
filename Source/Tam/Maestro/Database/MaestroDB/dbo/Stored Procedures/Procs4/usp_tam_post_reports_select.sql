CREATE PROCEDURE usp_tam_post_reports_select
(
	@id Int
)
AS
SELECT
	*
FROM
	tam_post_reports WITH(NOLOCK)
WHERE
	id = @id
