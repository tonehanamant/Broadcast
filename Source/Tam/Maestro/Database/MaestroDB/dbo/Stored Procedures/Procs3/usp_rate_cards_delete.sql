CREATE PROCEDURE [dbo].[usp_rate_cards_delete]
(
	@id Int)
AS
DELETE FROM rate_cards WHERE id=@id
