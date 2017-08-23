CREATE PROCEDURE [dbo].[usp_rate_card_versions_select]
(
	@id Int
)
AS
SELECT
	id,
	name,
	is_default
FROM
	rate_card_versions
WHERE
	id = @id
