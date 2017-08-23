CREATE PROCEDURE [dbo].[usp_RCS_GetDisplayTrafficRateCardsByTopography]
	@topography_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		trc.id, 
		topos.[name],
		trc.start_date,
		trc.end_date,
		trc.[name],
		e.firstname + ' ' + e.lastname [employee],
		trc.date_approved, 
		trc.date_created, 
		trc.date_last_modified		
	FROM
		traffic_rate_cards trc	(NOLOCK)
		JOIN 
			topographies topos (NOLOCK) on topos.id=trc.topography_id
		LEFT JOIN 
			employees e (NOLOCK) ON e.id=trc.approved_by_employee_id
	WHERE
		trc.topography_id=@topography_id
	ORDER BY
		convert(varchar, start_date, 110) + ' - ' +
		CASE
			WHEN 
				end_date is NULL
			THEN 
				'Present'
			ELSE
				convert(varchar, end_date, 110)
		END DESC
END
