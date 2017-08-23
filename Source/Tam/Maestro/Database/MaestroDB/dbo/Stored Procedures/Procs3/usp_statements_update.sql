CREATE PROCEDURE usp_statements_update
(
	@id		Int,
	@media_month_id		Int,
	@statement_type		TinyInt
)
AS
UPDATE statements SET
	media_month_id = @media_month_id,
	statement_type = @statement_type
WHERE
	id = @id

