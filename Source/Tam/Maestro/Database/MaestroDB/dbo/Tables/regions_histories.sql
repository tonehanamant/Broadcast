CREATE TABLE [dbo].[regions_histories] (
    [region_id]   INT           NOT NULL,
    [division_id] INT           NOT NULL,
    [code]        VARCHAR (15)  NULL,
    [name]        VARCHAR (255) NULL,
    [start_date]  DATETIME      NOT NULL,
    [end_date]    DATETIME      NULL,
    [is_active]   BIT           CONSTRAINT [DF_regions_histories_IsActive] DEFAULT ((0)) NULL,
    CONSTRAINT [pk_regions_histories] PRIMARY KEY CLUSTERED ([region_id] ASC, [division_id] ASC, [start_date] ASC),
    CONSTRAINT [FK_regions_histories_division_id] FOREIGN KEY ([division_id]) REFERENCES [dbo].[divisions] ([id]),
    CONSTRAINT [FK_regions_histories_region_id] FOREIGN KEY ([region_id]) REFERENCES [dbo].[regions] ([id])
);

