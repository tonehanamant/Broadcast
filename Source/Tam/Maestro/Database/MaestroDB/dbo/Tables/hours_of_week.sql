CREATE TABLE [dbo].[hours_of_week] (
    [hour_of_week] TINYINT NOT NULL,
    [mon]          BIT     NOT NULL,
    [tue]          BIT     NOT NULL,
    [wed]          BIT     NOT NULL,
    [thu]          BIT     NOT NULL,
    [fri]          BIT     NOT NULL,
    [sat]          BIT     NOT NULL,
    [sun]          BIT     NOT NULL,
    [start_time]   INT     NOT NULL,
    [end_time]     INT     NOT NULL,
    CONSTRAINT [PK_hours_of_week] PRIMARY KEY CLUSTERED ([hour_of_week] ASC) WITH (FILLFACTOR = 90)
);

