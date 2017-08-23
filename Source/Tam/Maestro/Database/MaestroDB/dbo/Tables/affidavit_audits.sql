CREATE TABLE [dbo].[affidavit_audits] (
    [media_month_id]       SMALLINT      NOT NULL,
    [affidavit_id]         BIGINT        NOT NULL,
    [date_created]         DATETIME      DEFAULT (getdate()) NOT NULL,
    [employee_id]          INT           NOT NULL,
    [affidavit_field_code] TINYINT       NOT NULL,
    [old_value]            VARCHAR (255) NOT NULL,
    [new_value]            VARCHAR (255) NOT NULL,
    CONSTRAINT [PK_affidavit_audits] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [date_created] ASC, [affidavit_id] ASC, [affidavit_field_code] ASC) WITH (IGNORE_DUP_KEY = ON) ON [MediaMonthSmallintScheme] ([media_month_id])
);

