
CREATE PROCEDURE [dbo].[usp_cmw_divisions_delete]
(
	@id Int
)
AS
DELETE FROM cmw_divisions WHERE id=@id

