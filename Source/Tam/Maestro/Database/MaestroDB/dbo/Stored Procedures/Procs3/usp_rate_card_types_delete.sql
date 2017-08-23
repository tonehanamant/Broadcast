CREATE PROCEDURE [dbo].[usp_rate_card_types_delete]
(
	@id Int)
AS
DELETE FROM rate_card_types WHERE id=@id
