CREATE PROCEDURE usp_coverage_universes_delete
(
	@id Int
)
AS
DELETE FROM coverage_universes WHERE id=@id
