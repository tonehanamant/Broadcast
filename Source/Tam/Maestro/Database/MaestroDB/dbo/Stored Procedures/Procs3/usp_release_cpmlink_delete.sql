CREATE PROCEDURE usp_release_cpmlink_delete
(
	@id Int
)
AS
DELETE FROM release_cpmlink WHERE id=@id
