CREATE PROCEDURE usp_account_statuses_select
(
	@id Int
)
AS
SELECT
	*
FROM
	account_statuses WITH(NOLOCK)
WHERE
	id = @id
