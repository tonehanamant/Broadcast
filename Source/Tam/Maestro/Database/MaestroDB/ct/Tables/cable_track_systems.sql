CREATE TABLE [ct].[cable_track_systems] (
    [tam_media_month]  VARCHAR (4)    NULL,
    [market]           NVARCHAR (255) NULL,
    [syscode]          VARCHAR (4)    NULL,
    [sysname]          NVARCHAR (255) NULL,
    [sys_aiue]         FLOAT (53)     NULL,
    [mso_owner_name]   NVARCHAR (255) NULL,
    [system_type_name] NVARCHAR (255) NULL,
    [contact_name]     NVARCHAR (255) NULL,
    [tp_name]          NVARCHAR (255) NULL,
    [tp_phone]         NVARCHAR (255) NULL,
    [tp_fax]           NVARCHAR (255) NULL,
    [tp_address_1]     NVARCHAR (255) NULL,
    [tp_address_2]     NVARCHAR (255) NULL,
    [city]             NVARCHAR (255) NULL,
    [state]            NVARCHAR (255) NULL,
    [zip_code]         FLOAT (53)     NULL,
    [tp_deadline]      NVARCHAR (255) NULL,
    [order_deadline]   NVARCHAR (255) NULL,
    [tape_size]        NVARCHAR (255) NULL,
    [flag]             NVARCHAR (255) NULL,
    [code_]            NVARCHAR (255) NULL,
    [tp_name_]         NVARCHAR (255) NULL,
    [address_1]        NVARCHAR (255) NULL,
    [address_2]        NVARCHAR (255) NULL,
    [city_]            NVARCHAR (255) NULL,
    [state_]           NVARCHAR (255) NULL,
    [zip_code_]        FLOAT (53)     NULL,
    [unknow_code]      FLOAT (53)     NULL
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'tam_media_month';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'tam_media_month';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'market';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'market';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'syscode';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'syscode';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'sysname';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'sysname';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'sys_aiue';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'sys_aiue';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'mso_owner_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'mso_owner_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'system_type_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'system_type_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'contact_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'contact_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'tp_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'tp_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'tp_phone';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'tp_phone';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'tp_fax';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'tp_fax';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'tp_address_1';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'tp_address_1';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'tp_address_2';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'tp_address_2';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'city';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'city';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'state';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'state';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'zip_code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'zip_code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'tp_deadline';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'tp_deadline';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'order_deadline';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'order_deadline';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'tape_size';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'tape_size';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'flag';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'flag';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'code_';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'code_';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'tp_name_';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'tp_name_';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'address_1';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'address_1';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'address_2';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'address_2';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'city_';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'city_';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'state_';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'state_';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'zip_code_';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'zip_code_';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'unknow_code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_systems', @level2type = N'COLUMN', @level2name = N'unknow_code';

