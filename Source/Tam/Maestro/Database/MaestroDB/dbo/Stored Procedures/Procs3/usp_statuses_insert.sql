CREATE PROCEDURE usp_statuses_insert
(
	@id		Int		OUTPUT,
	@status_set		VarChar(15),
	@name		VarChar(15),
	@description		VarChar(63)
)
AS
INSERT INTO statuses
(
	status_set,
	name,
	description
)
VALUES
(
	@status_set,
	@name,
	@description
)

SELECT
	@id = SCOPE_IDENTITY()

