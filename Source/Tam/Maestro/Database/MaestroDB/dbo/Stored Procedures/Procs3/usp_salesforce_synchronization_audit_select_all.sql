
CREATE PROCEDURE [dbo].[usp_salesforce_synchronization_audit_select_all]
AS
SELECT
	*
FROM
	salesforce_synchronization_audit WITH(NOLOCK)

