CREATE PROCEDURE usp_tam_post_plan_types_delete
(
	@id		TinyInt)
AS
BEGIN
DELETE FROM
	dbo.tam_post_plan_types
WHERE
	id = @id
END
