
CREATE PROCEDURE [dbo].[usp_salesforce_synchronization_exclusions_select_all]
AS
SELECT
	*
FROM
	salesforce_synchronization_exclusions WITH(NOLOCK)

