CREATE PROCEDURE usp_account_statuses_select_all
AS
SELECT
	*
FROM
	account_statuses WITH(NOLOCK)
