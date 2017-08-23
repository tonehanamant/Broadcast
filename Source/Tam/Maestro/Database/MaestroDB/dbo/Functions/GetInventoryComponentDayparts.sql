-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/2/2015
-- Description:	Returns the 16 component dayparts used by the inventory management system. 8 M-F, 8 SA-SU in 3 hour increments
-- =============================================
CREATE FUNCTION [dbo].[GetInventoryComponentDayparts]
(
)
RETURNS @dayparts TABLE
(
	id INT,
	start_time INT,
	end_time INT,
	mon INT,
	tue INT,
	wed INT,
	thu INT,
	fri INT,
	sat INT,
	sun INT,
	total_hours INT
)
AS
BEGIN
	INSERT INTO @dayparts
		SELECT
			d.id,
			d.start_time,
			d.end_time,
			d.mon,
			d.tue,
			d.wed,
			d.thu,
			d.fri,
			d.sat,
			d.sun,
			d.total_hours
		FROM
			vw_ccc_daypart d
		WHERE
			--M-F 3 hour increments
			(d.start_time=0 AND d.end_time=10799 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=10800 AND d.end_time=21599 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=21600 AND d.end_time=32399 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=32400 AND d.end_time=43199 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=43200 AND d.end_time=53999 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=54000 AND d.end_time=64799 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=64800 AND d.end_time=75599 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=75600 AND d.end_time=86399 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			--SA-SU 3 hour increments
			OR (d.start_time=0 AND d.end_time=10799 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=10800 AND d.end_time=21599 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=21600 AND d.end_time=32399 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=32400 AND d.end_time=43199 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=43200 AND d.end_time=53999 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=54000 AND d.end_time=64799 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=64800 AND d.end_time=75599 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=75600 AND d.end_time=86399 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
		ORDER BY
			d.mon,
			d.start_time;
	RETURN;
END