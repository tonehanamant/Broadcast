CREATE PROCEDURE usp_tam_post_plan_types_select_all
AS
BEGIN
SELECT
	*
FROM
	dbo.tam_post_plan_types WITH(NOLOCK)
END
