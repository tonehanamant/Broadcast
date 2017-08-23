CREATE PROCEDURE usp_network_breaks_delete
(
	@id Int
,
	@effective_date DateTime
)
AS
UPDATE network_breaks SET active=0, effective_date=@effective_date WHERE id=@id
