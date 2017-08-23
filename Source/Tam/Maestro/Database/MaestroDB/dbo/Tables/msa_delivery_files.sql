CREATE TABLE [dbo].[msa_delivery_files] (
    [id]                     INT      IDENTITY (1, 1) NOT NULL,
    [document_id]            INT      NOT NULL,
    [employee_id]            INT      NOT NULL,
    [date_completed]         DATETIME NOT NULL,
    [media_month_start_date] DATETIME NOT NULL,
    [media_month_end_date]   DATETIME NOT NULL,
    CONSTRAINT [PK_msa_delivery_files] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_msa_delivery_files_documents] FOREIGN KEY ([document_id]) REFERENCES [dbo].[documents] ([id]),
    CONSTRAINT [FK_msa_delivery_files_employees] FOREIGN KEY ([employee_id]) REFERENCES [dbo].[employees] ([id])
);

