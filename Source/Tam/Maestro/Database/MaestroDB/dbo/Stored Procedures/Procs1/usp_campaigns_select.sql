CREATE PROCEDURE usp_campaigns_select
(
	@id Int
)
AS
SELECT
	*
FROM
	campaigns WITH(NOLOCK)
WHERE
	id = @id
