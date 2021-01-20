CREATE TABLE [dbo].[vpvhs] (
    [id]           INT        IDENTITY (1, 1) NOT NULL,
    [quarter]      INT        NOT NULL,
    [year]         INT        NOT NULL,
    [audience_id]  INT        NOT NULL,
    [am_news]      FLOAT (53) NOT NULL,
    [pm_news]      FLOAT (53) NOT NULL,
    [syn_all]      FLOAT (53) NOT NULL,
    [vpvh_file_id] INT        NOT NULL,
    CONSTRAINT [PK_vpvhs] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_audiences_vpvhs] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_vpvh_files_vpvhs] FOREIGN KEY ([vpvh_file_id]) REFERENCES [dbo].[vpvh_files] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_vpvh_files_vpvhs]
    ON [dbo].[vpvhs]([vpvh_file_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_audiences_vpvhs]
    ON [dbo].[vpvhs]([audience_id] ASC);

