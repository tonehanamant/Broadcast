CREATE TABLE [dbo].[msa_deliveries_out] (
    [media_month_id]        SMALLINT   NOT NULL,
    [tam_post_proposal_id]  INT        NOT NULL,
    [tam_post_affidavit_id] BIGINT     NOT NULL,
    [audience_id]           INT        NOT NULL,
    [msa_delivery_files_id] INT        NOT NULL,
    [is_equivalized]        BIT        NOT NULL,
    [delivery]              FLOAT (53) NOT NULL,
    [msa_material_id]       INT        NULL,
    CONSTRAINT [PK_msa_deliveries_out] PRIMARY KEY NONCLUSTERED ([media_month_id] ASC, [tam_post_proposal_id] ASC, [tam_post_affidavit_id] ASC, [audience_id] ASC) WITH (FILLFACTOR = 90) ON [MediaMonthSmallintScheme] ([media_month_id]),
    CONSTRAINT [FK_msa_deliveries_msa_deliveries_out] FOREIGN KEY ([msa_delivery_files_id]) REFERENCES [dbo].[msa_delivery_files] ([id])
) ON [MediaMonthSmallintScheme] ([media_month_id]);



