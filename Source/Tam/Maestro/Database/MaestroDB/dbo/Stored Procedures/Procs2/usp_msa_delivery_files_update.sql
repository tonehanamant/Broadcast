-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/06/2014 09:28:41 AM
-- Description:	Auto-generated method to update a msa_delivery_files record.
-- =============================================
CREATE PROCEDURE usp_msa_delivery_files_update
	@id INT,
	@document_id INT,
	@employee_id INT,
	@date_completed DATETIME,
	@media_month_start_date DATETIME,
	@media_month_end_date DATETIME
AS
BEGIN
	UPDATE
		[dbo].[msa_delivery_files]
	SET
		[document_id]=@document_id,
		[employee_id]=@employee_id,
		[date_completed]=@date_completed,
		[media_month_start_date]=@media_month_start_date,
		[media_month_end_date]=@media_month_end_date
	WHERE
		[id]=@id
END
