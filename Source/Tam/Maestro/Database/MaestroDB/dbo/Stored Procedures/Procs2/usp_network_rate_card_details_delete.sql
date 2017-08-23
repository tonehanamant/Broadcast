



CREATE PROCEDURE usp_network_rate_card_details_delete
(
	@id Int
)
AS
BEGIN
DELETE FROM dbo.network_rate_card_details WHERE id=@id
END
