CREATE PROCEDURE usp_contact_methods_select_all
AS
SELECT
	*
FROM
	contact_methods WITH(NOLOCK)
