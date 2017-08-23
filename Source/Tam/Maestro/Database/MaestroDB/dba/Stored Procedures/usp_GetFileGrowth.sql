
--Create stored procedure for major production databases that we need to capture file growth using the following query

CREATE PROCEDURE [dba].[usp_GetFileGrowth] AS 
BEGIN 
-- RECORD all the database stats into daily_file_growth_report
 

SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;


WITH CTE_Disk_Info
( DBName, Drive, DrvFreeSpaceInMB, DrvTotalSpaceInMB )
AS 
(
SELECT DISTINCT DB_NAME(dovs.database_id) ,
LEFT(dovs.volume_mount_point,1) ,
CONVERT(INT,dovs.available_bytes/1048576.0) ,
CONVERT(INT,dovs.total_bytes/1048576.0) 
FROM sys.master_files mf
CROSS APPLY sys.dm_os_volume_stats(mf.database_id, mf.FILE_ID) dovs
WHERE DB_NAME(dovs.database_id) = DB_NAME() 
)
,
CTE_File_Group_Info
(DBName,File_Group,Logic_Name,[Filename],[CurrAllocatedSpaceInMB],[SpaceUsedInMB], [AvailSpaceInMB], [MaxSizeInMB],[Drive])
AS
(
SELECT
DB_NAME(),
ISNULL(b.groupname,'T-LOG') ,
Name,
[Filename],
CONVERT (Decimal(15,2),ROUND(a.Size/128.000,2)) ,
CONVERT (Decimal(15,2),ROUND(FILEPROPERTY(a.Name,'SpaceUsed')/128.000,2)),
CONVERT (Decimal(15,2),ROUND((a.Size-FILEPROPERTY(a.Name,'SpaceUsed'))/128.000,2)),
CONVERT (Decimal(15,2),ROUND(a.maxsize/128.000,2)) ,
UPPER(LEFT([Filename],1))
FROM dbo.sysfiles a 
LEFT OUTER JOIN sysfilegroups b  ON a.groupid = b.groupid
)
INSERT INTO dba.dbo.daily_file_growth_report (
	[dbname] ,
	[file_group] ,
	[logical_name] ,
	[drive]  ,
	[full_file_name] ,
	[allocated_space] ,
	[used_space] ,
	[available_space] ,
	[maximum_size] ,
	[drvfreespace] ,
	[drvtotalspace]  )

SELECT fg.DBName, fg.File_Group, fg.Logic_Name,fg.[Drive],fg.[Filename],fg.[CurrAllocatedSpaceInMB],fg.[SpaceUsedInMB], fg.[AvailSpaceInMB], 
fg.[MaxSizeInMB],di.DrvFreeSpaceInMB, di.DrvTotalSpaceInMB
FROM CTE_File_Group_Info fg 
JOIN CTE_Disk_Info di ON fg.DBName = di.DBName AND fg.Drive = di.Drive


END

