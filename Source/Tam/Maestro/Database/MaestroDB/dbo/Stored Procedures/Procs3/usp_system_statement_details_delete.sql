CREATE PROCEDURE usp_system_statement_details_delete
(
	@system_statement_id		Int,
	@date_sent		DateTime)
AS
DELETE FROM
	system_statement_details
WHERE
	system_statement_id = @system_statement_id
 AND
	date_sent = @date_sent
