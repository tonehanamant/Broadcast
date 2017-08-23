CREATE PROCEDURE usp_businesses_select_all
AS
SELECT
	*
FROM
	businesses WITH(NOLOCK)
