CREATE PROCEDURE usp_networks_select
(
	@id Int
)
AS
BEGIN
SELECT
	*
FROM
	dbo.networks WITH(NOLOCK)
WHERE
	id = @id
END
