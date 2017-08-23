CREATE PROCEDURE usp_traffic_rate_cards_update
(
	@id		Int,
	@topography_id		Int,
	@start_date		DateTime,
	@end_date		DateTime,
	@name		VarChar(127),
	@date_created		DateTime,
	@date_last_modified		DateTime,
	@date_approved		DateTime,
	@approved_by_employee_id		Int
)
AS
UPDATE traffic_rate_cards SET
	topography_id = @topography_id,
	start_date = @start_date,
	end_date = @end_date,
	name = @name,
	date_created = @date_created,
	date_last_modified = @date_last_modified,
	date_approved = @date_approved,
	approved_by_employee_id = @approved_by_employee_id
WHERE
	id = @id

