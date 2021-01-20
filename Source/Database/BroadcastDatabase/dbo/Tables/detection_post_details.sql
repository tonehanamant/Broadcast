CREATE TABLE [dbo].[detection_post_details] (
    [detection_file_detail_id] INT        NOT NULL,
    [audience_id]              INT        NOT NULL,
    [delivery]                 FLOAT (53) NOT NULL,
    [audience_rank]            INT        NULL,
    CONSTRAINT [PK_detection_post_details] PRIMARY KEY CLUSTERED ([detection_file_detail_id] ASC, [audience_id] ASC),
    CONSTRAINT [FK_detection_post_details_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_detection_post_details_detection_file_details] FOREIGN KEY ([detection_file_detail_id]) REFERENCES [dbo].[detection_file_details] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_detection_post_details_audiences]
    ON [dbo].[detection_post_details]([audience_id] ASC);

