CREATE PROCEDURE usp_tam_post_exclusion_summaries_select
(
	@id Int
)
AS
SELECT
	*
FROM
	tam_post_exclusion_summaries WITH(NOLOCK)
WHERE
	id = @id
