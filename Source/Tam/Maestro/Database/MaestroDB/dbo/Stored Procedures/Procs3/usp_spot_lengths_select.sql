CREATE PROCEDURE usp_spot_lengths_select
(
	@id Int
)
AS
SELECT
	*
FROM
	spot_lengths WITH(NOLOCK)
WHERE
	id = @id
