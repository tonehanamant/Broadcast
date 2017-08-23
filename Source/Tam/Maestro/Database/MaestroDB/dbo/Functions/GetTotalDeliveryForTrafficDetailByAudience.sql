CREATE FUNCTION [dbo].[GetTotalDeliveryForTrafficDetailByAudience]
(
                @traffic_detail_id INT,
                @audience_id INT
)
RETURNS @delivery TABLE
(
                traffic_detail_id INT,
                topography_id INT,
                delivery FLOAT
)
AS
BEGIN
                                INSERT INTO @delivery (traffic_detail_id, topography_id, delivery)
                                select 
                                                td.id, tdt.topography_id, ((tda.traffic_rating * dbo.GetTrafficDetailCoverageUniverse(@traffic_detail_id, @audience_id, tdt.topography_id)) / 1000.0) * sum(tdt.spots) 
                                from
                                                traffic_details td WITH (NOLOCK) 
                                                join traffic_detail_weeks tdw WITH (NOLOCK) on td.id = tdw.traffic_detail_id
                                                join traffic_detail_topographies tdt WITH (NOLOCK) on tdt.traffic_detail_week_id = tdw.id
                                                join traffic_detail_audiences tda WITH (NOLOCK) on tda.traffic_detail_id = td.id
                                where
                                                td.id = @traffic_detail_id
                                                and
                                                tda.audience_id = @audience_id
                                group by
                                                td.id, tdt.topography_id, tda.traffic_rating;
                                
                                RETURN;
END
