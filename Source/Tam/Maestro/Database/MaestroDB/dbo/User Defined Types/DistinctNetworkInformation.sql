CREATE TYPE [dbo].[DistinctNetworkInformation] AS TABLE (
    [network_id]                  INT        NOT NULL,
    [nielsen_network_id]          INT        NOT NULL,
    [nielsen_network_id_delivery] INT        NOT NULL,
    [nielsen_network_id_universe] INT        NOT NULL,
    [delivery_factor]             FLOAT (53) NOT NULL,
    [universe_factor]             FLOAT (53) NOT NULL,
    [network_rule_type]           TINYINT    NOT NULL,
    [bias]                        FLOAT (53) NOT NULL,
    PRIMARY KEY CLUSTERED ([network_id] ASC));

