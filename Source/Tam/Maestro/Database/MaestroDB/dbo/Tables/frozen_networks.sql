CREATE TABLE [dbo].[frozen_networks] (
    [media_month_id]        SMALLINT     NOT NULL,
    [id]                    INT          NOT NULL,
    [code]                  VARCHAR (15) NOT NULL,
    [name]                  VARCHAR (63) NOT NULL,
    [flag]                  TINYINT      NULL,
    [language_id]           TINYINT      NOT NULL,
    [affiliated_network_id] INT          NULL,
    [network_type_id]       TINYINT      NULL,
    CONSTRAINT [PK_frozen_networks] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [id] ASC)
);

