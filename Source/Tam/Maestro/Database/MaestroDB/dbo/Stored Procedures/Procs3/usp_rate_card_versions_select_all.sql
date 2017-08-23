CREATE PROCEDURE [dbo].[usp_rate_card_versions_select_all]
AS
SELECT
	id,
	name,
	is_default
FROM
	rate_card_versions
