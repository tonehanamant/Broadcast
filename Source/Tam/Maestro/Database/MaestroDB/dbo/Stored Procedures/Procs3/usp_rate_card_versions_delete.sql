CREATE PROCEDURE [dbo].[usp_rate_card_versions_delete]
(
	@id Int)
AS
DELETE FROM rate_card_versions WHERE id=@id
