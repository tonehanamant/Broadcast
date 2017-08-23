CREATE PROCEDURE [dbo].[usp_affidavit_delivery_run_messages_insert]
(
	@affidavit_delivery_run_id		Int,
	@date_created		DateTime,
	@note		VarChar(4095)
)
AS
INSERT INTO affidavit_delivery_run_messages
(
	affidavit_delivery_run_id,
	date_created,
	note
)
VALUES
(
	@affidavit_delivery_run_id,
	@date_created,
	@note
)
