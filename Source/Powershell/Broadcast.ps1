#SQLServerName
$SQLServer ="cadsql03\cadsql03qa"
#Database Name 
$DatabaseName ="Broadcast"
#Script path
$File ="\\devvm01tfs05\dropfolder\OnDeck\database\broadcast.sql"
#Execute sql file
invoke-sqlcmd –ServerInstance $SQLServer -Database $DatabaseName -InputFile $File
#Print file name which is executed
$filename 