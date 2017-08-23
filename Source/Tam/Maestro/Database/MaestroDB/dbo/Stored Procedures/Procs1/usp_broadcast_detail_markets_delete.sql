CREATE PROCEDURE [dbo].[usp_broadcast_detail_markets_delete]
(
	@id Int)
AS
DELETE FROM broadcast_detail_markets WHERE id=@id

