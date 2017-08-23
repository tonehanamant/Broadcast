CREATE PROCEDURE usp_business_histories_delete
(
	@business_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	business_histories
WHERE
	business_id = @business_id
 AND
	start_date = @start_date
