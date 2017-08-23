CREATE PROCEDURE usp_zones_delete
(
	@id Int
,
	@effective_date DateTime
)
AS
BEGIN
UPDATE zones SET active=0, effective_date=@effective_date WHERE id=@id
END
