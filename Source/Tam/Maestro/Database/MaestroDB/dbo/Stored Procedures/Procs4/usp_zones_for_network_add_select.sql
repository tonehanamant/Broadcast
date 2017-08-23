	CREATE PROCEDURE usp_zones_for_network_add_select  
	(  
	 @network_id int  
	)  
	AS  
	BEGIN  
	  
		SELECT z.* 
		FROM zones z (nolock)
		WHERE z.id NOT IN (SELECT zone_id 
						FROM uvw_zonenetwork_universe (nolock)
						WHERE network_id = @network_id)
		AND z.[type] IN ('Soft Interconnect'
						,'Interconnect'
						,'Zone Group'
						,'Local'
						,'Digital'
						,'Alternate Delivery System')
	END 
