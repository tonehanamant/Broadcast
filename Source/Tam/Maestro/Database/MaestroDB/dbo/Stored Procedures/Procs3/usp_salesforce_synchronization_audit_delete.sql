
CREATE PROCEDURE [dbo].[usp_salesforce_synchronization_audit_delete]
(
	@synchronization_type		VarChar(50),
	@audit_date		DateTime)
AS
DELETE FROM
	salesforce_synchronization_audit
WHERE
	synchronization_type = @synchronization_type
 AND
	audit_date = @audit_date

