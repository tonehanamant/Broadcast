CREATE TABLE [dbo].[broadcast_post_details] (
    [media_month_id]              INT        NOT NULL,
    [broadcast_affidavit_file_id] INT        NOT NULL,
    [broadcast_affidavit_id]      BIGINT     NOT NULL,
    [dma_id]                      INT        NOT NULL,
    [audience_id]                 INT        NOT NULL,
    [delivery]                    FLOAT (53) NOT NULL,
    CONSTRAINT [PK_broadcast_post_details] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [broadcast_affidavit_file_id] ASC, [broadcast_affidavit_id] ASC, [dma_id] ASC, [audience_id] ASC) ON [MediaMonthScheme] ([media_month_id]),
    CONSTRAINT [FK_broadcast_post_details_broadcast_affidavit_files] FOREIGN KEY ([dma_id]) REFERENCES [dbo].[dmas] ([id]),
    CONSTRAINT [FK_broadcast_post_details_broadcast_affidavits] FOREIGN KEY ([media_month_id], [broadcast_affidavit_id]) REFERENCES [dbo].[broadcast_affidavits] ([media_month_id], [id]),
    CONSTRAINT [FK_broadcast_post_details_media_months] FOREIGN KEY ([media_month_id]) REFERENCES [dbo].[media_months] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_post_details';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_post_details';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_post_details';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_post_details', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_post_details', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_post_details', @level2type = N'COLUMN', @level2name = N'broadcast_affidavit_file_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_post_details', @level2type = N'COLUMN', @level2name = N'broadcast_affidavit_file_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_post_details', @level2type = N'COLUMN', @level2name = N'broadcast_affidavit_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_post_details', @level2type = N'COLUMN', @level2name = N'broadcast_affidavit_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_post_details', @level2type = N'COLUMN', @level2name = N'dma_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_post_details', @level2type = N'COLUMN', @level2name = N'dma_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_post_details', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_post_details', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_post_details', @level2type = N'COLUMN', @level2name = N'delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_post_details', @level2type = N'COLUMN', @level2name = N'delivery';

