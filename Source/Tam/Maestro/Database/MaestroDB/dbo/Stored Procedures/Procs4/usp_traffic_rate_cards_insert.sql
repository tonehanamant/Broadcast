CREATE PROCEDURE usp_traffic_rate_cards_insert
(
	@id		Int		OUTPUT,
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
INSERT INTO traffic_rate_cards
(
	topography_id,
	start_date,
	end_date,
	name,
	date_created,
	date_last_modified,
	date_approved,
	approved_by_employee_id
)
VALUES
(
	@topography_id,
	@start_date,
	@end_date,
	@name,
	@date_created,
	@date_last_modified,
	@date_approved,
	@approved_by_employee_id
)

SELECT
	@id = SCOPE_IDENTITY()

