--exec [dbo].[spIndex_report] 13,10 --time
CREATE PROCEDURE [dbo].[spIndex_report]
@DB_ID int,
@frag_in_percent int
AS

SET NOCOUNT ON;

--select * from sys.databases
--dba 13
--maestro 7
--postlog_staing 8
--external_rating 14
--rentrak 12

delete from dba.dbo.daily_index_data_report_new
where database_name=DB_NAME(@DB_ID)

insert into dba.dbo.daily_index_data_report_new(database_name, table_name, index_name, 
partition_number, page_count, avg_fragmentation_in_percent, partition_row_count, index_type_desc, 
user_seeks, user_scans, user_updates, last_user_update)
select DB_NAME(s.database_id) AS database_name, object_name(s.object_id) as table_name, isnull(i.name, 'Heap Table') as index_name, 
 s.partition_number, s.page_count, s.avg_fragmentation_in_percent, SUM(p.rows) AS partition_row_count, s.index_type_desc,
 su.user_seeks, su.user_scans, su.user_updates, su.last_user_update  
from sys.dm_db_index_physical_stats 
			(@DB_ID, NULL, NULL, NULL, 'limited') as s  
	 join sys.indexes i 
	 on s.object_id=i.object_id 
	 AND s.index_id=i.index_id
	 JOIN sys.partitions p
	 ON s.partition_number=p.partition_number
	 AND p.object_id=s.object_id
	 AND p.index_id = i.index_id
	 JOIN sys.dm_db_index_usage_stats su
	 ON su.object_id=s.object_id
	 AND su.index_id=i.index_id
where s.index_type_desc in ('CLUSTERED INDEX', 'NONCLUSTERED INDEX')
	and s.avg_fragmentation_in_percent >@frag_in_percent
	and s.page_count>1000
GROUP BY DB_NAME(s.database_id) , object_name(s.object_id) , isnull(i.name, 'Heap Table'), 
 s.partition_number, s.page_count, s.avg_fragmentation_in_percent, s.index_type_desc,
 su.user_seeks, su.user_scans, su.user_updates, su.last_user_update
order by 6 desc



