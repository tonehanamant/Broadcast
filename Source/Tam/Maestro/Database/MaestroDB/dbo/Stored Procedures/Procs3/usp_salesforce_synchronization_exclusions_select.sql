
CREATE PROCEDURE [dbo].[usp_salesforce_synchronization_exclusions_select]
(
	@salesforce_synchronization_id		Int,
	@excluded_id		Int
)
AS
SELECT
	*
FROM
	salesforce_synchronization_exclusions WITH(NOLOCK)
WHERE
	salesforce_synchronization_id=@salesforce_synchronization_id
	AND
	excluded_id=@excluded_id


