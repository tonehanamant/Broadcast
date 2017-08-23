CREATE PROCEDURE usp_dayparts_select
(
	@id Int
)
AS
SELECT
	*
FROM
	dayparts WITH(NOLOCK)
WHERE
	id = @id
