CREATE PROCEDURE usp_reels_insert
(
	@id		Int		OUTPUT,
	@name		VarChar(63),
	@description		VarChar(127),
	@status_code		TinyInt,
	@has_screener		Bit,
	@date_finalized		DateTime,
	@date_created		DateTime,
	@date_last_modified		DateTime
)
AS
INSERT INTO reels
(
	name,
	description,
	status_code,
	has_screener,
	date_finalized,
	date_created,
	date_last_modified
)
VALUES
(
	@name,
	@description,
	@status_code,
	@has_screener,
	@date_finalized,
	@date_created,
	@date_last_modified
)

SELECT
	@id = SCOPE_IDENTITY()

