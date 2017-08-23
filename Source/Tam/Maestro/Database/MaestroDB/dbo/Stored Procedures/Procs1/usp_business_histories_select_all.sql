CREATE PROCEDURE usp_business_histories_select_all
AS
SELECT
	*
FROM
	business_histories WITH(NOLOCK)
