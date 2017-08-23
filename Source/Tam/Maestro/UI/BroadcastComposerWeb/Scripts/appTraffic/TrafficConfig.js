var TrafficConfig = {
    getTrafficGridConfig: function () {
        var gridCfg = {
            name: 'TrafficGrid',
            columns: [
                { field: 'ID', caption: 'ID', size: '25%', sortable: true, searchable: true },
                { field: 'Name', caption: 'Name', size: '25%', sortable: true, searchable: true },
                { field: 'OmUnassignedIscis', caption: 'OM Unassigned ISCIs', size: '25%', sortable: true, searchable: true },
                { field: 'Flight', caption: 'Flight', size: '25%', sortable: true, searchable: true }
            ]
        };

        return gridCfg;
    }
}