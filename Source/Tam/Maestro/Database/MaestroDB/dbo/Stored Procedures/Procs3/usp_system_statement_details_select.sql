CREATE PROCEDURE usp_system_statement_details_select
(
	@system_statement_id		Int,
	@date_sent		DateTime
)
AS
SELECT
	*
FROM
	system_statement_details WITH(NOLOCK)
WHERE
	system_statement_id=@system_statement_id
	AND
	date_sent=@date_sent

