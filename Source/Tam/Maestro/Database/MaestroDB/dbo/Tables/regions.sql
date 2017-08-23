CREATE TABLE [dbo].[regions] (
    [id]             INT           IDENTITY (1, 1) NOT NULL,
    [division_id]    INT           NULL,
    [code]           VARCHAR (15)  NULL,
    [name]           VARCHAR (255) NULL,
    [effective_date] DATETIME      NULL,
    [is_active]      BIT           DEFAULT ((0)) NULL,
    PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_regions_divisions_id] FOREIGN KEY ([division_id]) REFERENCES [dbo].[divisions] ([id])
);

