CREATE PROCEDURE [dbo].[usp_msa_posts_delete]
(
	@id Int
)
AS
DELETE FROM dbo.msa_posts WHERE id=@id
