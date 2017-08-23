CREATE TABLE [dbo].[system_component_parameters] (
    [component_id]       VARCHAR (50)  NOT NULL,
    [parameter_key]      VARCHAR (50)  NOT NULL,
    [parameter_value]    VARCHAR (500) NULL,
    [parameter_type]     VARCHAR (50)  NOT NULL,
    [description]        VARCHAR (500) NOT NULL,
    [last_modified_time] DATETIME      NOT NULL,
    CONSTRAINT [PK_system_component_parameters] PRIMARY KEY CLUSTERED ([component_id] ASC, [parameter_key] ASC) WITH (FILLFACTOR = 90)
);





