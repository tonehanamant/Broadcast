
CREATE PROCEDURE [dbo].[usp_salesforce_synchronization_delete]
(
	@id Int
)
AS
DELETE FROM salesforce_synchronization WHERE id=@id

