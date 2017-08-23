CREATE PROCEDURE usp_progress_billing_types_select_all
AS
SELECT
	*
FROM
	progress_billing_types WITH(NOLOCK)
