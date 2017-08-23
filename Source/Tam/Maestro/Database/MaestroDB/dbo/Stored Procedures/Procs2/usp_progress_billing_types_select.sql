CREATE PROCEDURE usp_progress_billing_types_select
(
	@id Int
)
AS
SELECT
	*
FROM
	progress_billing_types WITH(NOLOCK)
WHERE
	id = @id
