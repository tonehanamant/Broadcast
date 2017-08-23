CREATE PROCEDURE [dbo].[usp_affidavit_delivery_run_messages_select]
(
	@affidavit_delivery_run_id		Int,
	@date_created		DateTime
)
AS
SELECT
	*
FROM
	affidavit_delivery_run_messages WITH(NOLOCK)
WHERE
	affidavit_delivery_run_id=@affidavit_delivery_run_id
	AND
	date_created=@date_created
