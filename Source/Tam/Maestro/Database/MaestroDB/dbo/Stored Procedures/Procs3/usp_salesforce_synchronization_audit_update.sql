
CREATE PROCEDURE [dbo].[usp_salesforce_synchronization_audit_update]
(
	@id		Int,
	@synchronization_type		VarChar(50),
	@last_sync_date		DateTime,
	@modified_date		DateTime,
	@modified_by		VarChar(50),
	@audit_operation		Char(1),
	@audit_date		DateTime
)
AS
UPDATE salesforce_synchronization_audit SET
	id = @id,
	last_sync_date = @last_sync_date,
	modified_date = @modified_date,
	modified_by = @modified_by,
	audit_operation = @audit_operation
WHERE
	synchronization_type = @synchronization_type AND
	audit_date = @audit_date

