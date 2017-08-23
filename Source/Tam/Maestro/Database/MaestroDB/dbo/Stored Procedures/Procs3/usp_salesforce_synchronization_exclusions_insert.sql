
CREATE PROCEDURE [dbo].[usp_salesforce_synchronization_exclusions_insert]
(
	@salesforce_synchronization_id		Int,
	@excluded_id		Int
)
AS
INSERT INTO salesforce_synchronization_exclusions
(
	salesforce_synchronization_id,
	excluded_id
)
VALUES
(
	@salesforce_synchronization_id,
	@excluded_id
)


