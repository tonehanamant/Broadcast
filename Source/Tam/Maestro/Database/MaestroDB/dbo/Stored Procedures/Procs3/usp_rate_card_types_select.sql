CREATE PROCEDURE [dbo].[usp_rate_card_types_select]
(
	@id Int
)
AS
SELECT
	id,
	name,
	is_default
FROM
	rate_card_types
WHERE
	id = @id
