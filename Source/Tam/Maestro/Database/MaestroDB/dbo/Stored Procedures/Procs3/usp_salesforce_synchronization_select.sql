
CREATE PROCEDURE [dbo].[usp_salesforce_synchronization_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	salesforce_synchronization WITH(NOLOCK)
WHERE
	id = @id

