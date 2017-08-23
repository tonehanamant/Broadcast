CREATE PROCEDURE usp_releases_update
(
	@id		Int,
	@category_id		Int,
	@status_id		Int,
	@name		VarChar(63),
	@description		VarChar(63),
	@notes		VarChar(2047),
	@release_date		DateTime,
	@confirm_by_date		DateTime
)
AS
UPDATE releases SET
	category_id = @category_id,
	status_id = @status_id,
	name = @name,
	description = @description,
	notes = @notes,
	release_date = @release_date,
	confirm_by_date = @confirm_by_date
WHERE
	id = @id

