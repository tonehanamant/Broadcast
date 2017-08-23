CREATE PROCEDURE usp_zone_types_select
(
	@id Int
)
AS
BEGIN
SELECT
	*
FROM
	zone_types WITH(NOLOCK)
WHERE
	id = @id
END
