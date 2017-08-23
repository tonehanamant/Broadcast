-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Modified:	12/12/2013 - Added primary flag and added NOLOCK's
-- Description:	Returns zone_histories as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[GetZoneNetworksAsOf]
(	
	@dateAsOf as datetime
)
RETURNS @as_of TABLE
(
	zone_id int, 
	network_id int, 
	source varchar(15), 
	trafficable bit, 
	[primary] bit, 
	subscribers int, 
	as_of_date datetime
) 
AS
BEGIN
	insert into
		@as_of(
			zone_id, network_id, source, trafficable, [primary], subscribers, as_of_date
		)
		select
			zone_id, network_id, source, trafficable, [primary], subscribers, @dateAsOf as_of_date
		from
			zone_networks (NOLOCK)
		where
			@dateAsOf >= effective_date

		union

		select
			zone_id, network_id, source, trafficable, [primary], subscribers, @dateAsOf as_of_date
		from
			zone_network_histories (NOLOCK)
		where
			@dateAsOf between start_date and end_date

	RETURN;
END
