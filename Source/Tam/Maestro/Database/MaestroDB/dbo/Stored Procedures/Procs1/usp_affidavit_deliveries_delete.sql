
CREATE PROCEDURE [dbo].[usp_affidavit_deliveries_delete]
(
	@affidavit_id		BigInt,
	@media_month_id		Int,
	@audience_id		Int,
	@rating_source_id		TinyInt)
AS
DELETE FROM
	affidavit_deliveries
WHERE
	affidavit_id = @affidavit_id
 AND
	media_month_id = @media_month_id
 AND
	audience_id = @audience_id
 AND
	rating_source_id = @rating_source_id

