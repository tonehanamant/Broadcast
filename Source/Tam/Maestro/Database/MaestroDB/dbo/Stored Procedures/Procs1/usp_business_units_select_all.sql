CREATE PROCEDURE [dbo].[usp_business_units_select_all]
AS
SELECT
	*
FROM
	business_units WITH(NOLOCK)
