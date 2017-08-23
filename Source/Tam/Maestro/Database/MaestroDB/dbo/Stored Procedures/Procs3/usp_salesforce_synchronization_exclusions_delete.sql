
CREATE PROCEDURE [dbo].[usp_salesforce_synchronization_exclusions_delete]
(
	@salesforce_synchronization_id		Int,
	@excluded_id		Int)
AS
DELETE FROM
	salesforce_synchronization_exclusions
WHERE
	salesforce_synchronization_id = @salesforce_synchronization_id
 AND
	excluded_id = @excluded_id

