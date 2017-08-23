CREATE PROCEDURE usp_statements_insert
(
	@id		Int		OUTPUT,
	@media_month_id		Int,
	@statement_type		TinyInt
)
AS
INSERT INTO statements
(
	media_month_id,
	statement_type
)
VALUES
(
	@media_month_id,
	@statement_type
)

SELECT
	@id = SCOPE_IDENTITY()

