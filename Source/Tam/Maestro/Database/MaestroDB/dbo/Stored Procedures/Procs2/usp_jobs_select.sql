CREATE PROCEDURE usp_jobs_select
(
	@id Int
)
AS
SELECT
	*
FROM
	jobs WITH(NOLOCK)
WHERE
	id = @id
