CREATE PROCEDURE usp_jobs_update
(
	@id		Int,
	@type		VarChar(31),
	@priority		TinyInt,
	@status		VarChar(31),
	@date_created		DateTime,
	@date_started		DateTime,
	@date_completed		DateTime
)
AS
UPDATE jobs SET
	type = @type,
	priority = @priority,
	status = @status,
	date_created = @date_created,
	date_started = @date_started,
	date_completed = @date_completed
WHERE
	id = @id

