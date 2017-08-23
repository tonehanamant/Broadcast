CREATE PROCEDURE usp_timespans_select
(
	@id Int
)
AS
SELECT
	*
FROM
	timespans WITH(NOLOCK)
WHERE
	id = @id
