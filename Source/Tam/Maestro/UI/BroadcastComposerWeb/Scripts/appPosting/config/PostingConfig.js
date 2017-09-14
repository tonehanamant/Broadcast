var PostingConfig = {

    getCfg: function (name, obj) {
        var item = this[name];
        if (item) {
            return util.copy.Data(item, obj, null, true);
        }

        return null;
    },

    copyConfigItem: function (item, xtra) {
        return util.copyData(item, extra, null, true);
    },

    getPostingGridConfig: function (view) {
        var self = this;
        var gridCfg = {
            name: 'PostGrid',
            show: {
                footer: true
            },
            menu: [
                { id: 1, text: 'File Settings' },
                { id: 2, text: 'Post Report' },
                { id: 3, text: 'Delete' }
            ],
            columns: [
                { field: 'FileName', caption: 'File Name', size: '25%', sortable: true, searchable: true },
                { field: 'DisplayDemo', caption: 'Demos', size: '25%', sortable: true, searchable: true },
                {
                    field: 'UploadDate', caption: 'Upload Date', size: '25%', sortable: true, searchable: false, render: function (record, index, column_index) {
                        return moment(record.UploadDate).format("M/D/YYYY");
                    }
                },
                {
                    field: 'ModifiedDate', caption: 'Last Modified', size: '25%', sortable: true, searchable: false, render: function (record, index, column_index) {
                        return moment(record.ModifiedDate).format("M/D/YYYY");
                    }
                }
            ],
            sortData: [{ field: 'UploadDate', direction: 'desc' }]
        };

        return gridCfg;
    }
}