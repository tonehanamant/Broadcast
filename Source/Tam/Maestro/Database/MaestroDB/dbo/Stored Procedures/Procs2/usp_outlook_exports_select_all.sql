CREATE PROCEDURE usp_outlook_exports_select_all
AS
SELECT
	*
FROM
	outlook_exports WITH(NOLOCK)
