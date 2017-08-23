CREATE PROCEDURE [dbo].[usp_broadcast_proposals_delete]
(
	@id Int)
AS
DELETE FROM broadcast_proposals WHERE id=@id

