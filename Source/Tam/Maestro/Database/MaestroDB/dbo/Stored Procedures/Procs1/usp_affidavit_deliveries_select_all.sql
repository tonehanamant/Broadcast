CREATE PROCEDURE usp_affidavit_deliveries_select_all
AS
SELECT
	*
FROM
	affidavit_deliveries WITH(NOLOCK)
