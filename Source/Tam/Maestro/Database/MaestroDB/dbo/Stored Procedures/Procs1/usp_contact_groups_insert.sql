CREATE PROCEDURE usp_contact_groups_insert
(
	@id		Int		OUTPUT,
	@parent_contact_group_id		Int,
	@name		VarChar(63),
	@description		VarChar(255)
)
AS
INSERT INTO contact_groups
(
	parent_contact_group_id,
	name,
	description
)
VALUES
(
	@parent_contact_group_id,
	@name,
	@description
)

SELECT
	@id = SCOPE_IDENTITY()

