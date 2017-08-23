-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/13/2016 10:15:55 AM
-- Description:	Auto-generated method to update a regions_histories record.
-- =============================================
create PROCEDURE dbo.usp_regions_histories_update
	@id INT,
	@division_id INT,
	@code VARCHAR(15),
	@start_date DATETIME,
	@end_date DATETIME
AS
BEGIN
	UPDATE
		[dbo].[regions_histories]
	SET
		[division_id]=@division_id,
		[code]=@code,
		[start_date]=@start_date,
		[end_date]=@end_date
	WHERE
		[id]=@id
END