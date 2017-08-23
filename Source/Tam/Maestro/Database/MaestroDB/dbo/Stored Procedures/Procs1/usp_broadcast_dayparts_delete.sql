
CREATE PROCEDURE [dbo].[usp_broadcast_dayparts_delete]
(
	@id Int
)
AS
DELETE FROM broadcast_dayparts WHERE id=@id

