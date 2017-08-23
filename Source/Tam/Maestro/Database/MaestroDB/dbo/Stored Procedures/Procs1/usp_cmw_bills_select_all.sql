CREATE PROCEDURE usp_cmw_bills_select_all
AS
SELECT
	*
FROM
	cmw_bills WITH(NOLOCK)
