CREATE PROCEDURE [dbo].[usp_affidavit_delivery_run_messages_delete]
(
	@affidavit_delivery_run_id		Int,
	@date_created		DateTime)
AS
DELETE FROM
	affidavit_delivery_run_messages
WHERE
	affidavit_delivery_run_id = @affidavit_delivery_run_id
 AND
	date_created = @date_created
