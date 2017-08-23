CREATE PROCEDURE usp_zone_business_histories_delete
(
	@zone_id		Int,
	@business_id		Int,
	@start_date		DateTime,
	@type		VarChar(15))
AS
DELETE FROM
	zone_business_histories
WHERE
	zone_id = @zone_id
 AND
	business_id = @business_id
 AND
	start_date = @start_date
 AND
	type = @type
