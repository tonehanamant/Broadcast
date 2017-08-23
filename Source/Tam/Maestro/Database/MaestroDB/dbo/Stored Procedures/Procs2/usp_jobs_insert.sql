CREATE PROCEDURE usp_jobs_insert
(
	@id		Int		OUTPUT,
	@type		VarChar(31),
	@priority		TinyInt,
	@status		VarChar(31),
	@date_created		DateTime,
	@date_started		DateTime,
	@date_completed		DateTime
)
AS
INSERT INTO jobs
(
	type,
	priority,
	status,
	date_created,
	date_started,
	date_completed
)
VALUES
(
	@type,
	@priority,
	@status,
	@date_created,
	@date_started,
	@date_completed
)

SELECT
	@id = SCOPE_IDENTITY()

