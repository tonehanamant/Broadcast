CREATE PROCEDURE [dbo].[usp_broadcast_proposal_details_delete]
(
	@id Int
)
AS
DELETE FROM broadcast_proposal_details WHERE id=@id

