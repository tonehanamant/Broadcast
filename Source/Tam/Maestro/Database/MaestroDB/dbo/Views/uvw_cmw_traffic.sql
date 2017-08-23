
CREATE VIEW [dbo].[uvw_cmw_traffic]
as
WITH ID_Table(id, version)
as
(SELECT 		
		CASE
			WHEN cmw_traffic.original_cmw_traffic_id is null
				THEN cmw_traffic.id
			ELSE
				cmw_traffic.original_cmw_traffic_id
		END as id,
		max(version_number)
FROM cmw_traffic (nolock)
GROUP BY
	CASE
		WHEN cmw_traffic.original_cmw_traffic_id is null
			THEN cmw_traffic.id
		ELSE
			cmw_traffic.original_cmw_traffic_id
	END 
)
select 		
	cmw_traffic.id,
	ID_Table.id 'sort_id',
	CASE
		WHEN original_cmw_traffic_id is null
			THEN CAST(cmw_traffic.id as varchar)
		ELSE
			CAST(original_cmw_traffic_id as varchar) + '-R' + CAST(cmw_traffic.version_number as varchar)
	END 'display_id',
	agency_cmw_traffic_company_id, advertiser_cmw_traffic_company_id, cmw_traffic_product_id, system_id, zone_id, network_id, status_id, 
	cmw_traffic_product_description_id, coverage_universe, order_date, release_name, release_date, start_date, end_date, notes, flight_text, 
	network_handles_copy, date_created, date_last_modified, salesperson_employee_id,original_cmw_traffic_id, approved_by_employee_id, 
	approved_date, cmw_contact_id,cmw_traffic.version_number, internal_notes, original_cmw_traffic_status_id
	FROM cmw_traffic (nolock)
		JOIN ID_Table ON ID_Table.id = 
	CASE 
		WHEN cmw_traffic.original_cmw_traffic_id is null 
			THEN cmw_traffic.id 
			ELSE cmw_traffic.original_cmw_traffic_id 
	END 
		AND cmw_traffic.version_number = ID_Table.version
