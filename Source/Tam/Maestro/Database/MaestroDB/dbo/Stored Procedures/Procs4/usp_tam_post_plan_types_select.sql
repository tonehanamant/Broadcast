CREATE PROCEDURE usp_tam_post_plan_types_select
(
	@id		TinyInt
)
AS
BEGIN
SELECT
	*
FROM
	dbo.tam_post_plan_types WITH(NOLOCK)
WHERE
	id=@id

END
