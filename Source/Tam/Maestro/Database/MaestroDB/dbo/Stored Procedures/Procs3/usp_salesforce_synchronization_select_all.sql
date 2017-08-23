
CREATE PROCEDURE [dbo].[usp_salesforce_synchronization_select_all]
AS
SELECT
	*
FROM
	salesforce_synchronization WITH(NOLOCK)

