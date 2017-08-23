CREATE PROCEDURE usp_states_select
(
	@id Int
)
AS
SELECT
	*
FROM
	states WITH(NOLOCK)
WHERE
	id = @id
