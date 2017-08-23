CREATE PROCEDURE usp_contact_groups_update
(
	@id		Int,
	@parent_contact_group_id		Int,
	@name		VarChar(63),
	@description		VarChar(255)
)
AS
UPDATE contact_groups SET
	parent_contact_group_id = @parent_contact_group_id,
	name = @name,
	description = @description
WHERE
	id = @id

