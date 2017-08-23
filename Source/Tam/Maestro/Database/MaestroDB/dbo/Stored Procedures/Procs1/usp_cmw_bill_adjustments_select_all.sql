CREATE PROCEDURE usp_cmw_bill_adjustments_select_all
AS
SELECT
	*
FROM
	cmw_bill_adjustments WITH(NOLOCK)
