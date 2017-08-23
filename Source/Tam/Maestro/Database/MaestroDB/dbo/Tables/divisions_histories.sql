CREATE TABLE [dbo].[divisions_histories] (
    [division_id] INT           NOT NULL,
    [code]        VARCHAR (15)  NULL,
    [name]        VARCHAR (255) NULL,
    [mvpd]        INT           NULL,
    [start_date]  DATETIME      NOT NULL,
    [end_date]    DATETIME      NULL,
    [is_active]   BIT           NULL,
    CONSTRAINT [pk_divisions_histories] PRIMARY KEY CLUSTERED ([division_id] ASC, [start_date] ASC),
    CONSTRAINT [FK_divisions_histories_division_id] FOREIGN KEY ([division_id]) REFERENCES [dbo].[divisions] ([id])
);

