CREATE PROCEDURE usp_cmw_bills_select
(
	@id Int
)
AS
SELECT
	*
FROM
	cmw_bills WITH(NOLOCK)
WHERE
	id = @id
