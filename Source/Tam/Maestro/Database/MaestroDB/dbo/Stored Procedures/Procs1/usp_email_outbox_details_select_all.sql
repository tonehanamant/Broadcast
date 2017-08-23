CREATE PROCEDURE usp_email_outbox_details_select_all
AS
SELECT
	*
FROM
	email_outbox_details WITH(NOLOCK)
