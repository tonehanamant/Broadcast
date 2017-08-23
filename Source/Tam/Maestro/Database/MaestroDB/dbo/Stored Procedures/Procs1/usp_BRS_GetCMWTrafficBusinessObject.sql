
CREATE PROCEDURE [dbo].[usp_BRS_GetCMWTrafficBusinessObject]
@cmwTrafficID int

AS
BEGIN

	SET NOCOUNT ON;

	declare @originalCMWTrafficID int
	set @originalCMWTrafficID = (select original_cmw_traffic_id from cmw_traffic where id = @cmwTrafficID)

	exec dbo.usp_cmw_traffic_select @cmwTrafficID

	SELECT
		cmw_traffic_id,
		start_date,
		end_date,
		selected
	FROM
		cmw_traffic_flights (NOLOCK)
	WHERE
		cmw_traffic_id = @cmwTrafficID

	SELECT
		cmw_traffic_id,
		cmw_material_id,
		rotation,
		disposition,
		sensitive,
		comment,
		start_date,
		end_date,
		cmw_materials.code,
		spot_lengths.length
	FROM
		cmw_traffic_materials (NOLOCK)
	JOIN cmw_materials (NOLOCK) on cmw_materials.id = cmw_traffic_materials.cmw_material_id
	JOIN spot_lengths (NOLOCK) on spot_lengths.id = cmw_materials.spot_length_id
	WHERE
		cmw_traffic_id = @cmwTrafficID

	SELECT
		details.id,
		details.cmw_traffic_id,
		details.daypart_id,
		details.spot_length_id,
		details.unit_cost,
		details.total_units,
		details.suspend_date,
		details.start_date,
		details.end_date,
		details.original_cmw_traffic_detail_id,
		details.status_code,
		details.line_number,
		d.id,
		d.code,
		d.[name],
		d.start_time,
		d.end_time,
		d.mon,
		d.tue,
		d.wed,
		d.thu,
		d.fri,
		d.sat,
		d.sun,
		spot_lengths.id,
		spot_lengths.length,
		spot_lengths.delivery_multiplier,
		spot_lengths.order_by,
		spot_lengths.is_default
	FROM
		cmw_traffic_details details (NOLOCK)
	JOIN vw_ccc_daypart d (NOLOCK) on d.id=details.daypart_id
	JOIN spot_lengths (NOLOCK) on details.spot_length_id=spot_lengths.id
	WHERE
		details.cmw_traffic_id = @cmwTrafficID
	ORDER BY 
		CASE 
			WHEN 
				details.line_number IS NULL 
			THEN 
				details.id
			ELSE 
				details.line_number
		END ASC
		

	SELECT
		days.cmw_traffic_details_id,
		days.day_id,
		days.units,
		days.is_max
	FROM
		cmw_traffic_detail_days days (NOLOCK)
	JOIN cmw_traffic_details (NOLOCK) on cmw_traffic_details.id=days.cmw_traffic_details_id
	WHERE
		cmw_traffic_details.cmw_traffic_id = @cmwTrafficID

	SELECT *
	FROM
		cmw_traffic (nolock)
	WHERE
		(((@originalCMWTrafficID is null) 
	AND original_cmw_traffic_id = @cmwTrafficID)
	OR
		(id <> @cmwTrafficID
	AND
		(id = @originalCMWTrafficID 
	OR
		original_cmw_traffic_id = @originalCMWTrafficID)))

END
