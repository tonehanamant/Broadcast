
-- =============================================
-- Author:		CRUD Creator
-- Create date: 09/16/2016 09:55:45 AM
-- Description:	Auto-generated method to update a cluster_rate_cards record.
-- =============================================
CREATE PROCEDURE usp_cluster_rate_cards_update
	@id INT,
	@topography_id INT,
	@start_date DATETIME,
	@end_date DATETIME,
	@name VARCHAR(127),
	@date_created DATETIME,
	@date_last_modified DATETIME,
	@date_approved DATETIME,
	@approved_by_employee_id INT
AS
BEGIN
	UPDATE
		[dbo].[cluster_rate_cards]
	SET
		[topography_id]=@topography_id,
		[start_date]=@start_date,
		[end_date]=@end_date,
		[name]=@name,
		[date_created]=@date_created,
		[date_last_modified]=@date_last_modified,
		[date_approved]=@date_approved,
		[approved_by_employee_id]=@approved_by_employee_id
	WHERE
		[id]=@id
END