-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/06/2014 09:28:41 AM
-- Description:	Auto-generated method to insert a msa_delivery_files record.
-- =============================================
CREATE PROCEDURE usp_msa_delivery_files_insert
	@id INT OUTPUT,
	@document_id INT,
	@employee_id INT,
	@date_completed DATETIME,
	@media_month_start_date DATETIME,
	@media_month_end_date DATETIME
AS
BEGIN
	INSERT INTO [dbo].[msa_delivery_files]
	(
		[document_id],
		[employee_id],
		[date_completed],
		[media_month_start_date],
		[media_month_end_date]
	)
	VALUES
	(
		@document_id,
		@employee_id,
		@date_completed,
		@media_month_start_date,
		@media_month_end_date
	)

	SELECT
		@id = SCOPE_IDENTITY()
END
