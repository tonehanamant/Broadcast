CREATE PROCEDURE usp_account_statuses_insert
(
	@id		Int		OUTPUT,
	@name		VarChar(127)
)
AS
INSERT INTO account_statuses
(
	name
)
VALUES
(
	@name
)

SELECT
	@id = SCOPE_IDENTITY()

