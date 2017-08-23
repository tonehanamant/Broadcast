CREATE PROCEDURE usp_timespans_delete
(
	@id Int
)
AS
DELETE FROM timespans WHERE id=@id
