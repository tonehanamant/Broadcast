CREATE TABLE [dbo].[materials] (
    [id]                   INT            IDENTITY (1, 1) NOT NULL,
    [product_id]           INT            NULL,
    [spot_length_id]       INT            NOT NULL,
    [code]                 VARCHAR (31)   NOT NULL,
    [original_code]        VARCHAR (31)   CONSTRAINT [DF_materials_original_code] DEFAULT ('') NOT NULL,
    [title]                VARCHAR (255)  NOT NULL,
    [type]                 VARCHAR (15)   NOT NULL,
    [url]                  VARCHAR (1023) NOT NULL,
    [phone_type]           TINYINT        NOT NULL,
    [phone_number]         VARCHAR (15)   NOT NULL,
    [date_received]        DATETIME       NULL,
    [date_created]         DATETIME       NOT NULL,
    [date_last_modified]   DATETIME       NOT NULL,
    [tape_log]             BIT            CONSTRAINT [DF_materials_tape_log] DEFAULT ((0)) NOT NULL,
    [tape_log_disposition] VARCHAR (1027) CONSTRAINT [DF_materials_tape_log_disposition] DEFAULT ('') NOT NULL,
    [active]               BIT            CONSTRAINT [DF_materials_active] DEFAULT ((1)) NOT NULL,
    [is_hd]                BIT            CONSTRAINT [DF_materials_is_hd] DEFAULT ((0)) NOT NULL,
    [is_house_isci]        BIT            CONSTRAINT [DF_materials_is_house_isci] DEFAULT ((0)) NOT NULL,
    [real_material_id]     INT            NULL,
    [has_screener]         BIT            CONSTRAINT [DF_materials_has_screener] DEFAULT ((0)) NOT NULL,
    [language_id]          TINYINT        CONSTRAINT [DF_materials_language_id] DEFAULT ((1)) NOT NULL,
    [sensitive]            BIT            CONSTRAINT [DF_materials_sensitive] DEFAULT ((0)) NOT NULL,
    [hd_material_id]       INT            NULL,
    CONSTRAINT [PK_materials] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_materials_products] FOREIGN KEY ([product_id]) REFERENCES [dbo].[products] ([id]),
    CONSTRAINT [FK_materials_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id]),
    CONSTRAINT [IX_materials] UNIQUE NONCLUSTERED ([code] ASC) WITH (FILLFACTOR = 90)
);




GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'product_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'product_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'spot_length_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'spot_length_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'original_code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'original_code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'title';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'title';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'type';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'type';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'url';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'url';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'phone_type';


GO
EXECUTE sp_addextendedproperty @name = N'Enum', @value = N'[DataContract]
public enum PhoneTypeEnum
{
[EnumMember]
Not_Applicable = 0,
[EnumMember]
Dedicated = 1,
[EnumMember]
Vanity = 2
}', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'phone_type';


GO
EXECUTE sp_addextendedproperty @name = N'EnumName', @value = N'PhoneTypeEnum', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'phone_type';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'phone_type';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'phone_number';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'phone_number';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'date_received';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'date_received';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'date_last_modified';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'date_last_modified';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'tape_log';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'tape_log';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'tape_log_disposition';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'tape_log_disposition';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'active';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'active';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'is_hd';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'is_hd';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'is_house_isci';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'is_house_isci';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'real_material_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'real_material_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'has_screener';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'has_screener';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'language_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'materials', @level2type = N'COLUMN', @level2name = N'language_id';

