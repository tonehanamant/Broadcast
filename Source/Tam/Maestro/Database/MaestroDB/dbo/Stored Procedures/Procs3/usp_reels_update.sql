CREATE PROCEDURE usp_reels_update
(
	@id		Int,
	@name		VarChar(63),
	@description		VarChar(127),
	@status_code		TinyInt,
	@has_screener		Bit,
	@date_finalized		DateTime,
	@date_created		DateTime,
	@date_last_modified		DateTime
)
AS
UPDATE reels SET
	name = @name,
	description = @description,
	status_code = @status_code,
	has_screener = @has_screener,
	date_finalized = @date_finalized,
	date_created = @date_created,
	date_last_modified = @date_last_modified
WHERE
	id = @id

