CREATE VIEW [dbo].[uvw_rptzonedma_universe]
AS
SELECT     
	zd.zone_id, 
	zd.dma_id, 
	zd.weight,  
	zd.effective_date 'start_date',  
	NULL AS end_date  
FROM  
	zone_dmas zd (NOLOCK)
WHERE
	zd.zone_id NOT IN (SELECT DISTINCT zone_id FROM rpt_zone_dmas)

UNION ALL

SELECT     
	zd.zone_id,  
	zd.dma_id,   
	zd.weight,   
	zd.start_date,   
	zd.end_date   
FROM   
	zone_dma_histories zd (NOLOCK)
WHERE
	zd.zone_id NOT IN (SELECT DISTINCT zone_id FROM rpt_zone_dmas)

UNION ALL

SELECT     
	rzd.zone_id, 
	rzd.dma_id, 
	rzd.weight,  
	rzd.effective_date 'start_date',
	NULL 'end_date'
FROM  
	rpt_zone_dmas rzd (NOLOCK)
