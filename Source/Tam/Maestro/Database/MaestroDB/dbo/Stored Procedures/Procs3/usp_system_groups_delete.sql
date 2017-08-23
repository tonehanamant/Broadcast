CREATE PROCEDURE usp_system_groups_delete
(
	@id Int
,
	@effective_date DateTime
)
AS
UPDATE system_groups SET active=0, effective_date=@effective_date WHERE id=@id
