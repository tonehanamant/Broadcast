﻿CREATE PROCEDURE usp_email_types_insert
(
	@id		Int		OUTPUT,
	@name		VarChar(63),
	@is_default		Bit
)
AS
INSERT INTO email_types
(
	name,
	is_default
)
VALUES
(
	@name,
	@is_default
)

SELECT
	@id = SCOPE_IDENTITY()

