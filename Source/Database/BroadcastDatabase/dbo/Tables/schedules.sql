CREATE TABLE [dbo].[schedules] (
    [id]               INT           IDENTITY (1, 1) NOT NULL,
    [estimate_id]      INT           NULL,
    [advertiser_id]    INT           NOT NULL,
    [name]             VARCHAR (127) NOT NULL,
    [posting_book_id]  INT           NOT NULL,
    [start_date]       DATETIME      NOT NULL,
    [end_date]         DATETIME      NOT NULL,
    [created_by]       VARCHAR (63)  NOT NULL,
    [created_date]     DATETIME      NOT NULL,
    [modified_by]      VARCHAR (63)  NOT NULL,
    [modified_date]    DATETIME      NOT NULL,
    [post_type]        TINYINT       NOT NULL,
    [inventory_source] TINYINT       NOT NULL,
    [equivalized]      BIT           NOT NULL,
    CONSTRAINT [PK_schedules] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_schedules_media_months] FOREIGN KEY ([posting_book_id]) REFERENCES [dbo].[media_months] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_schedules_media_months]
    ON [dbo].[schedules]([posting_book_id] ASC);

