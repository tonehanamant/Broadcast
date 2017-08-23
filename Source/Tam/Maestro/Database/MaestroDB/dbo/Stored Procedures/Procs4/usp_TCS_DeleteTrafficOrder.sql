

CREATE PROCEDURE [dbo].[usp_TCS_DeleteTrafficOrder]
	    @traffic_id INT
	AS
	BEGIN
		IF OBJECT_ID('tempdb..#temp_traffic_detail') IS NOT NULL DROP TABLE #temp_traffic_detail;
	
		if (select count(1) from dbo.traffic where original_traffic_id = @traffic_id) >1
		begin
		--	print 'must remove original_traffic_id first'
			RETURN
		end
	
		--temp table
		select id into #temp_traffic_detail from traffic_details with (NOLOCK) where traffic_id = @traffic_id
	
		DELETE FROM traffic_orders WHERE traffic_detail_id in (select id from #temp_traffic_detail);
		DELETE FROM traffic_cap_monthly_override_approvals WHERE traffic_id = @traffic_id;
		DELETE FROM traffic_monthly_cap_queues WHERE traffic_id = @traffic_id;
		DELETE FROM traffic_details_proposal_details_map WHERE traffic_detail_id in (select id from #temp_traffic_detail);
		DELETE FROM traffic_detail_audiences WHERE traffic_detail_id in (select id from #temp_traffic_detail);
		DELETE FROM traffic_detail_topographies WHERE traffic_detail_week_id in 
			(select traffic_detail_weeks.id from traffic_detail_weeks (NOLOCK) 
				JOIN traffic_details (NOLOCK) on traffic_details.id = traffic_detail_weeks.traffic_detail_id 
				where traffic_details.traffic_id = @traffic_id);
		DELETE FROM traffic_detail_weeks WHERE traffic_detail_id in (select id from #temp_traffic_detail);
		DELETE FROM traffic_audiences where traffic_id = @traffic_id;
		DELETE FROM traffic_flights where traffic_id = @traffic_id;
		DELETE FROM traffic_employees where traffic_id = @traffic_id;
		DELETE FROM traffic_alert_material_maps where traffic_material_id in (SELECT id from traffic_materials (NOLOCK) where traffic_id = @traffic_id);
		DELETE FROM traffic_material_flights where traffic_material_id in (SELECT id from traffic_materials (NOLOCK) where traffic_id = @traffic_id);
		DELETE FROM traffic_materials where traffic_id = @traffic_id;
		DELETE FROM traffic_proposals where traffic_id = @traffic_id;
		DELETE FROM traffic_details where traffic_id = @traffic_id;
		DELETE FROM release_cpmlink where traffic_id = @traffic_id;
		DELETE FROM traffic_master_alert_traffic_alerts WHERE traffic_alert_id in (SELECT id from traffic_alerts (NOLOCK) where traffic_id = @traffic_id);
		DELETE FROM traffic_alerts where traffic_id = @traffic_id;
		Update affidavits set traffic_id=Null where traffic_id = @traffic_id;
		UPDATE traffic set internal_note_id = null, external_note_id = null where id = @traffic_id;
		--delete from traffic  where original_traffic_id = @traffic_id;
		DELETE FROM NOTES WHERE note_type = 'traffic' AND reference_id = @traffic_id;
	
		IF OBJECT_ID('tempdb..#temp_traffic_detail') IS NOT NULL DROP TABLE #temp_traffic_detail;
	END
