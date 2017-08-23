CREATE PROCEDURE usp_property_histories_delete
(
	@property_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	property_histories
WHERE
	property_id = @property_id
 AND
	start_date = @start_date
