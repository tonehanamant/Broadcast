CREATE PROCEDURE usp_billing_cycles_select_all
AS
SELECT
	*
FROM
	billing_cycles WITH(NOLOCK)
