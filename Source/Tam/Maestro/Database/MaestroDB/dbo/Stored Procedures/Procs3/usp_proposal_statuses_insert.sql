CREATE PROCEDURE usp_proposal_statuses_insert
(
	@id		Int		OUTPUT,
	@code		VarChar(15),
	@name		VarChar(63),
	@is_default		Bit
)
AS
INSERT INTO proposal_statuses
(
	code,
	name,
	is_default
)
VALUES
(
	@code,
	@name,
	@is_default
)

SELECT
	@id = SCOPE_IDENTITY()

