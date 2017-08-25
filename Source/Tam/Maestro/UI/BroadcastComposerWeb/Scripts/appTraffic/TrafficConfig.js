var TrafficConfig = {
    getTrafficGridConfig: function () {
        var gridCfg = {
            name: 'TrafficGrid',
            columns: [
                { field: 'Id', caption: 'ID', size: '25%' },
                { field: 'Name', caption: 'Name', size: '25%' },
                { field: 'Advertiser', caption: 'Advertiser', size: '25%' },
                { field: 'OpenMarketUnassignedISCI', caption: 'OM Unassigned ISCIs', size: '25%' },
                { field: 'ProprietaryUnassignedISCI', caption: 'Proprietary Inventory Unassigned ISCIs', size: '25%' },
                { field: 'Flight', caption: 'Flight', size: '25%' }
            ],

            // disable nested grid toggle
            onResize: function (event) {
                for (var i = 0; i < this.records.length; i++) {
                    this.expand(this.records[i].recid);
                }

                $('.w2ui-icon-collapse').remove();
            }
        };

        return gridCfg;
    }
}