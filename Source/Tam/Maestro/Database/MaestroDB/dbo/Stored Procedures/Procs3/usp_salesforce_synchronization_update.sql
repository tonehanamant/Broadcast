
CREATE PROCEDURE [dbo].[usp_salesforce_synchronization_update]
(
	@id		Int,
	@synchronization_type		VarChar(50),
	@last_sync_date		DateTime,
	@modified_date		DateTime,
	@modified_by		VarChar(50)
)
AS
UPDATE salesforce_synchronization SET
	synchronization_type = @synchronization_type,
	last_sync_date = @last_sync_date,
	modified_date = @modified_date,
	modified_by = @modified_by
WHERE
	id = @id


