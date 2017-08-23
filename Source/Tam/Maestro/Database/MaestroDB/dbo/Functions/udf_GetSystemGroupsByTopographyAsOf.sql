
-- =============================================
-- Author:		John Carsley
-- Create date: 02/07/2013
-- Description:	Returns system_group_histories as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetSystemGroupsByTopographyAsOf]
(	
	@idTopography as int,
	@dateAsOf as datetime
)
RETURNS @system_groups TABLE
(
	system_group_id INT,
	name varchar(63),
	flag tinyint,
	as_of_date datetime
)
AS
BEGIN

	DECLARE @topographyIds UniqueIdTable
	
	insert into @topographyIds(id) values (@idTopography);
	
	insert @system_groups
	select
		sg.system_group_id,
		sg.name,
		sg.flag,
		sg.as_of_date
	from
		dbo.udf_GetSystemGroupsByTopographiesAsOf(@topographyIds, @dateAsOf) sg

	RETURN;
END
