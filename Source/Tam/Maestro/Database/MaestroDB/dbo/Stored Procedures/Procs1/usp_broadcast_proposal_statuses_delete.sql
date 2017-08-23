CREATE PROCEDURE [dbo].[usp_broadcast_proposal_statuses_delete]
(
	@id TinyInt)
AS
DELETE FROM broadcast_proposal_statuses WHERE id=@id

