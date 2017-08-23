
-- =============================================
-- Author:		CRUD Creator
-- Create date: 09/16/2016 09:55:45 AM
-- Description:	Auto-generated method to insert a cluster_rate_cards record.
-- =============================================
CREATE PROCEDURE usp_cluster_rate_cards_insert
	@id INT OUTPUT,
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
	INSERT INTO [dbo].[cluster_rate_cards]
	(
		[topography_id],
		[start_date],
		[end_date],
		[name],
		[date_created],
		[date_last_modified],
		[date_approved],
		[approved_by_employee_id]
	)
	VALUES
	(
		@topography_id,
		@start_date,
		@end_date,
		@name,
		@date_created,
		@date_last_modified,
		@date_approved,
		@approved_by_employee_id
	)

	SELECT
		@id = SCOPE_IDENTITY()
END