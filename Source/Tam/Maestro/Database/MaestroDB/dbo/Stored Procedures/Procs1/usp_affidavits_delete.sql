CREATE PROCEDURE usp_affidavits_delete
(
	@id BigInt
)
AS
DELETE FROM affidavits WHERE id=@id
