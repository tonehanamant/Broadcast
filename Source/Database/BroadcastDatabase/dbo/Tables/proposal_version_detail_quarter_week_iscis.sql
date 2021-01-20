CREATE TABLE [dbo].[proposal_version_detail_quarter_week_iscis] (
    [id]                                      INT           IDENTITY (1, 1) NOT NULL,
    [proposal_version_detail_quarter_week_id] INT           NOT NULL,
    [client_isci]                             VARCHAR (63)  NOT NULL,
    [house_isci]                              VARCHAR (63)  NOT NULL,
    [brand]                                   VARCHAR (127) NULL,
    [married_house_iscii]                     BIT           NOT NULL,
    [monday]                                  BIT           NULL,
    [tuesday]                                 BIT           NULL,
    [wednesday]                               BIT           NULL,
    [thursday]                                BIT           NULL,
    [friday]                                  BIT           NULL,
    [saturday]                                BIT           NULL,
    [sunday]                                  BIT           NULL,
    CONSTRAINT [PK_proposal_version_detail_quarter_week_iscis] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_proposal_version_detail_quarter_week_iscis_proposal_version_detail_quarter_weeks] FOREIGN KEY ([proposal_version_detail_quarter_week_id]) REFERENCES [dbo].[proposal_version_detail_quarter_weeks] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_detail_quarter_week_iscis_proposal_version_detail_quarter_weeks]
    ON [dbo].[proposal_version_detail_quarter_week_iscis]([proposal_version_detail_quarter_week_id] ASC);

