CREATE PROCEDURE usp_releases_insert
(
	@id		Int		OUTPUT,
	@category_id		Int,
	@status_id		Int,
	@name		VarChar(63),
	@description		VarChar(63),
	@notes		VarChar(2047),
	@release_date		DateTime,
	@confirm_by_date		DateTime
)
AS
INSERT INTO releases
(
	category_id,
	status_id,
	name,
	description,
	notes,
	release_date,
	confirm_by_date
)
VALUES
(
	@category_id,
	@status_id,
	@name,
	@description,
	@notes,
	@release_date,
	@confirm_by_date
)

SELECT
	@id = SCOPE_IDENTITY()

