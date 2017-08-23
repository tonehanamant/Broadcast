CREATE TABLE [dbo].[broadcast_scx] (
    [id]                          INT            IDENTITY (1, 1) NOT NULL,
    [broadcast_affidavit_file_id] INT            NOT NULL,
    [advertiser_id]               INT            NOT NULL,
    [agency_id]                   INT            NOT NULL,
    [product_id]                  INT            NOT NULL,
    [name]                        VARCHAR (1023) NULL,
    [document_date]               DATETIME       NULL,
    [start_date]                  DATE           NULL,
    [end_date]                    DATE           NULL,
    [buytype]                     VARCHAR (63)   NULL,
    [nsi_book_id]                 INT            NOT NULL,
    CONSTRAINT [PK_broadcast_scx] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_broadcast_scx_broadcast_affidavit_files] FOREIGN KEY ([broadcast_affidavit_file_id]) REFERENCES [dbo].[broadcast_affidavit_files] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_broadcast_scx_companies_advertiser] FOREIGN KEY ([advertiser_id]) REFERENCES [dbo].[companies] ([id]),
    CONSTRAINT [FK_broadcast_scx_companies_agency] FOREIGN KEY ([agency_id]) REFERENCES [dbo].[companies] ([id]),
    CONSTRAINT [FK_broadcast_scx_products] FOREIGN KEY ([product_id]) REFERENCES [dbo].[products] ([id])
);

