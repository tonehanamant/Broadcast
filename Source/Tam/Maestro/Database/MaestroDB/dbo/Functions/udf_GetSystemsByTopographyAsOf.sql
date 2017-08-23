
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns system_histories as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetSystemsByTopographyAsOf]
(	
	@idTopography as int,
	@dateAsOf as datetime
)
RETURNS @systems_table TABLE
(
	system_id int,
	code varchar(15),
	name varchar(63),
	location varchar(63),
	spot_yield_weight float,
	traffic_order_format int,
	flag tinyint,
	as_of_date datetime
)
AS
BEGIN

	DECLARE @topographyIds UniqueIdTable
	
	insert into @topographyIds(id) values (@idTopography);
	
	insert into @systems_table
	select
		s.system_id, 
		s.code, 
		s.name, 
		s.location, 
		s.spot_yield_weight, 
		s.traffic_order_format, 
		s.flag, 
		s.as_of_date
	from
		[dbo].[udf_GetSystemsByTopographiesAsOf](@topographyIds, @dateAsOf) s
	
	RETURN;
END;
