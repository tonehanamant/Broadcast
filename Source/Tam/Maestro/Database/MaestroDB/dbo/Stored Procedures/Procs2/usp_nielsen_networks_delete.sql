CREATE PROCEDURE usp_nielsen_networks_delete
(
	@id Int
,
	@effective_date DateTime
)
AS
UPDATE nielsen_networks SET active=0, effective_date=@effective_date WHERE id=@id
