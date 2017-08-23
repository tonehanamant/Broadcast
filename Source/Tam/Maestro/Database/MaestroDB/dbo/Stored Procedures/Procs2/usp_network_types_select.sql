CREATE PROCEDURE usp_network_types_select
(
	@id TinyInt
)
AS
BEGIN
SELECT
	*
FROM
	dbo.network_types WITH(NOLOCK)
WHERE
	id = @id
END
