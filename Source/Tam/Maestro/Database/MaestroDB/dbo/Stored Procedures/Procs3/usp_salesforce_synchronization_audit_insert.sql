
CREATE PROCEDURE [dbo].[usp_salesforce_synchronization_audit_insert]
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
INSERT INTO salesforce_synchronization_audit
(
	id,
	synchronization_type,
	last_sync_date,
	modified_date,
	modified_by,
	audit_operation,
	audit_date
)
VALUES
(
	@id,
	@synchronization_type,
	@last_sync_date,
	@modified_date,
	@modified_by,
	@audit_operation,
	@audit_date
)


