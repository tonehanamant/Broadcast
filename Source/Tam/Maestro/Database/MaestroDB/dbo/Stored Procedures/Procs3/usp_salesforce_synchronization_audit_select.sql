
CREATE PROCEDURE [dbo].[usp_salesforce_synchronization_audit_select]
(
	@synchronization_type		VarChar(50),
	@audit_date		DateTime
)
AS
SELECT
	*
FROM
	salesforce_synchronization_audit WITH(NOLOCK)
WHERE
	synchronization_type=@synchronization_type
	AND
	audit_date=@audit_date


