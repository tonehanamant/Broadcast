CREATE PROCEDURE usp_tam_post_plan_types_update
(
	@id		TinyInt,
	@plan_type_name		VarChar(100)
)
AS
BEGIN
UPDATE dbo.tam_post_plan_types SET
	plan_type_name = @plan_type_name
WHERE
	id = @id
END
