
CREATE PROCEDURE [dbo].[usp_invoice_types_select_all]
AS
SELECT
	*
FROM
	invoice_types WITH(NOLOCK)

