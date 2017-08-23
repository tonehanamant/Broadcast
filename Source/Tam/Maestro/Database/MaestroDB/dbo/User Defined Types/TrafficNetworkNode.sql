CREATE TYPE [dbo].[TrafficNetworkNode] AS TABLE (
    [daypart_id]        INT      NULL,
    [traffic_detail_id] INT      NULL,
    [spots]             INT      NULL,
    [start_date]        DATETIME NULL,
    [end_date]          DATETIME NULL,
    [topography_id]     INT      NULL);

