CREATE PROCEDURE usp_networks_delete
(
	@id Int
,
	@effective_date DateTime
)
AS
BEGIN
UPDATE dbo.networks SET active=0, effective_date=@effective_date WHERE id=@id
END
