CREATE PROCEDURE usp_jobs_delete
(
	@id Int
)
AS
DELETE FROM jobs WHERE id=@id
