CREATE PROCEDURE usp_businesses_delete
(
	@id Int
,
	@effective_date DateTime
)
AS
UPDATE businesses SET active=0, effective_date=@effective_date WHERE id=@id
