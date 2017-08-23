CREATE PROCEDURE usp_zones_select
(
	@id Int
)
AS
BEGIN
SELECT
	*
FROM
	zones WITH(NOLOCK)
WHERE
	id = @id
END
