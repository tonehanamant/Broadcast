CREATE PROCEDURE usp_rate_card_details_delete
(
	@id Int)
AS
DELETE FROM rate_card_details WHERE id=@id
