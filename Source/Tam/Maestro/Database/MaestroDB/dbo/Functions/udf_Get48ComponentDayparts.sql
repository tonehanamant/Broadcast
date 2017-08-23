-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/17/2016
-- Description:	Returns the 48 component dayparts used by the inventory management system. 24 M-F, 24 SA-SU in 1 hour increments
-- =============================================
CREATE FUNCTION [dbo].[udf_Get48ComponentDayparts]
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
			--M-F 1 hour increments
			(d.start_time=0 AND d.end_time=3599 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=3600 AND d.end_time=7199 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=7200 AND d.end_time=10799 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=10800 AND d.end_time=14399 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=14400 AND d.end_time=17999 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=18000 AND d.end_time=21599 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=21600 AND d.end_time=25199 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=25200 AND d.end_time=28799 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=28800 AND d.end_time=32399 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=32400 AND d.end_time=35999 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=36000 AND d.end_time=39599 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=39600 AND d.end_time=43199 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=43200 AND d.end_time=46799 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=46800 AND d.end_time=50399 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=50400 AND d.end_time=53999 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=54000 AND d.end_time=57599 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=57600 AND d.end_time=61199 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=61200 AND d.end_time=64799 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=64800 AND d.end_time=68399 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=68400 AND d.end_time=71999 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=72000 AND d.end_time=75599 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=75600 AND d.end_time=79199 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=79200 AND d.end_time=82799 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			OR (d.start_time=82800 AND d.end_time=86399 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=0 AND d.sun=0)
			--SA-SU 1 hour increments
			OR (d.start_time=0 AND d.end_time=3599 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=3600 AND d.end_time=7199 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=7200 AND d.end_time=10799 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=10800 AND d.end_time=14399 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=14400 AND d.end_time=17999 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=18000 AND d.end_time=21599 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=21600 AND d.end_time=25199 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=25200 AND d.end_time=28799 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=28800 AND d.end_time=32399 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=32400 AND d.end_time=35999 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=36000 AND d.end_time=39599 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=39600 AND d.end_time=43199 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=43200 AND d.end_time=46799 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=46800 AND d.end_time=50399 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=50400 AND d.end_time=53999 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=54000 AND d.end_time=57599 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=57600 AND d.end_time=61199 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=61200 AND d.end_time=64799 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=64800 AND d.end_time=68399 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=68400 AND d.end_time=71999 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=72000 AND d.end_time=75599 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=75600 AND d.end_time=79199 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=79200 AND d.end_time=82799 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
			OR (d.start_time=82800 AND d.end_time=86399 AND d.mon=0 AND d.tue=0 AND d.wed=0 AND d.thu=0 AND d.fri=0 AND d.sat=1 AND d.sun=1)
		ORDER BY
			d.mon,
			d.start_time;
	RETURN;
END