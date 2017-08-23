CREATE PROCEDURE [dbo].[usp_affidavit_delivery_run_messages_select_all]
AS
SELECT
	*
FROM
	affidavit_delivery_run_messages WITH(NOLOCK)
