CREATE PROCEDURE [dbo].[usp_business_units_select]
(
	@id TinyInt
)
AS
SELECT
	*
FROM
	business_units WITH(NOLOCK)
WHERE
	id = @id
