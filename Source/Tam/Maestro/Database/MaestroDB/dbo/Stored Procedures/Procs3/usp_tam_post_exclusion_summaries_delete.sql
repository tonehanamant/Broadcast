CREATE PROCEDURE usp_tam_post_exclusion_summaries_delete
(
	@id Int
)
AS
DELETE FROM tam_post_exclusion_summaries WHERE id=@id
