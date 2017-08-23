CREATE PROCEDURE usp_affidavits_select_all
AS
SELECT
	*
FROM
	affidavits WITH(NOLOCK)
