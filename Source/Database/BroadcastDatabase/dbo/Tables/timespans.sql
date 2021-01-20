CREATE TABLE [dbo].[timespans] (
    [id]         INT IDENTITY (1, 1) NOT NULL,
    [start_time] INT NOT NULL,
    [end_time]   INT NOT NULL,
    CONSTRAINT [PK_timespans] PRIMARY KEY CLUSTERED ([id] ASC)
);

