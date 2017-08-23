CREATE PROCEDURE usp_affidavit_deliveries_select
(
	@affidavit_id		BigInt,
	@audience_id		Int,
	@rating_category_id		Int
)
AS
SELECT
	*
FROM
	affidavit_deliveries WITH(NOLOCK)
WHERE
	affidavit_id=@affidavit_id
	AND
	audience_id=@audience_id
	AND
	rating_category_id=@rating_category_id