CREATE TABLE [dbo].[vpvh_quarters] (
    [id]          INT        IDENTITY (1, 1) NOT NULL,
    [audience_id] INT        NOT NULL,
    [year]        INT        NOT NULL,
    [quarter]     INT        NOT NULL,
    [pm_news]     FLOAT (53) NOT NULL,
    [am_news]     FLOAT (53) NOT NULL,
    [syn_all]     FLOAT (53) NOT NULL,
    [tdn]         FLOAT (53) NOT NULL,
    [tdns]        FLOAT (53) NOT NULL,
    CONSTRAINT [PK_vpvh_quarters] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_vpvh_quarters_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_vpvh_quarters_audiences]
    ON [dbo].[vpvh_quarters]([audience_id] ASC);

