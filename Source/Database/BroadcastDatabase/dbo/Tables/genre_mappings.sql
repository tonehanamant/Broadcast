CREATE TABLE [dbo].[genre_mappings] (
    [maestro_genre_id] INT          NOT NULL,
    [mapped_genre_id]  INT          NOT NULL,
    [created_by]       VARCHAR (63) NOT NULL,
    [created_date]     DATETIME     NOT NULL,
    [modified_by]      VARCHAR (63) NOT NULL,
    [modified_date]    DATETIME     NOT NULL,
    CONSTRAINT [PK_genre_mappings] PRIMARY KEY CLUSTERED ([maestro_genre_id] ASC, [mapped_genre_id] ASC),
    CONSTRAINT [FK__genre_map__maest__27114E05] FOREIGN KEY ([maestro_genre_id]) REFERENCES [dbo].[genres] ([id]),
    CONSTRAINT [FK__genre_map__mappe__2805723E] FOREIGN KEY ([mapped_genre_id]) REFERENCES [dbo].[genres] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK__genre_map__mappe__2805723E]
    ON [dbo].[genre_mappings]([mapped_genre_id] ASC);

