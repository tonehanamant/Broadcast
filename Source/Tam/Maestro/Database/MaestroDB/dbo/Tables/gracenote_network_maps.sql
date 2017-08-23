CREATE TABLE [dbo].[gracenote_network_maps] (
    [network_id]        INT          NOT NULL,
    [tz_time_zone_name] VARCHAR (45) NOT NULL,
    [tf_station_num]    INT          NOT NULL,
    [start_date]        DATETIME     NOT NULL,
    [end_date]          DATETIME     NULL,
    CONSTRAINT [PK_gracenote_network_maps] PRIMARY KEY CLUSTERED ([network_id] ASC, [tz_time_zone_name] ASC, [tf_station_num] ASC, [start_date] ASC)
);

