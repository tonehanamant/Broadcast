CREATE PROCEDURE usp_dmas_delete
(
	@id Int
,
	@effective_date DateTime
)
AS
UPDATE dmas SET active=0, effective_date=@effective_date WHERE id=@id
