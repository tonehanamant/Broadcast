-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/16/2008
-- Updated:		7/15/2015 - refactored to deduplicate code, instead we now just reuse dbo.GetDaypartComponents.
-- Description:	Looks up the component dayparts which would make up the daypart defined by the parameters.
-- =============================================
CREATE FUNCTION [dbo].[GetDaypartComponentsByDaypartId]
(	
	@daypart_id INT
)
RETURNS @dayparts TABLE
(
	id INT,
	code VARCHAR(15),
	name VARCHAR(63),
	tier INT,
	start_time INT,
	end_time INT,
	mon INT,
	tue INT,
	wed INT,
	thu INT,
	fri INT,
	sat INT,
	sun INT
)
AS
BEGIN
	DECLARE @start_time INT
	DECLARE @end_time INT
	DECLARE @monday INT
	DECLARE @tuesday INT
	DECLARE @wednesday INT
	DECLARE @thursday INT
	DECLARE @friday INT
	DECLARE @saturday INT
	DECLARE @sunday INT

	SELECT @start_time=start_time, @end_time=end_time, @monday=mon, @tuesday=tue, @wednesday=wed, @thursday=thu, @friday=fri, @saturday=sat, @sunday=sun FROM vw_ccc_daypart (NOLOCK) WHERE id=@daypart_id
	INSERT INTO @dayparts
		SELECT id,code,name,tier,start_time,end_time,mon,tue,wed,thu,fri,sat,sun FROM dbo.GetDaypartComponents(@start_time,@end_time,@monday,@tuesday,@wednesday,@thursday,@friday,@saturday,@sunday)
	RETURN;
END
