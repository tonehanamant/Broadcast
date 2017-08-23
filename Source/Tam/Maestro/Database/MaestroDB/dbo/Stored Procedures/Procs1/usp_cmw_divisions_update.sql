
CREATE PROCEDURE [dbo].[usp_cmw_divisions_update]
(
	@id		Int,
	@name		VarChar(50)
)
AS
UPDATE cmw_divisions SET
	name = @name
WHERE
	id = @id


