
-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/13/2016 10:15:54 AM
-- Description:	Auto-generated method to update a divisions_histories record.
-- =============================================
create PROCEDURE dbo.usp_divisions_histories_update
	@id INT,
	@zone_id INT,
	@code VARCHAR(15),
	@start_date DATETIME,
	@end_date DATETIME
AS
BEGIN
	UPDATE
		[dbo].[divisions_histories]
	SET
		[zone_id]=@zone_id,
		[code]=@code,
		[start_date]=@start_date,
		[end_date]=@end_date
	WHERE
		[id]=@id
END