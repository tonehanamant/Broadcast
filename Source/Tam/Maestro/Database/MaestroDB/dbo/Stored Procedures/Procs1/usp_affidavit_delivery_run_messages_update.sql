CREATE PROCEDURE [dbo].[usp_affidavit_delivery_run_messages_update]
(
	@affidavit_delivery_run_id		Int,
	@date_created		DateTime,
	@note		VarChar(4095)
)
AS
UPDATE affidavit_delivery_run_messages SET
	note = @note
WHERE
	affidavit_delivery_run_id = @affidavit_delivery_run_id AND
	date_created = @date_created

