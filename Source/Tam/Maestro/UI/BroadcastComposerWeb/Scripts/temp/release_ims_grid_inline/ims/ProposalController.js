//specific controller for standalone SPotLimit (from System Composer)
var ProposalController = Class.extend({
    // The datamember holding the options for the proposal, in the future, we can change this to the knockout style and remove it
    proposalOptions: {
        RoundProposalRate: false
    },

    apiChangeRoundingRate: function (boolean) {
        if (boolean) {
            this.proposalOptions.RoundProposalRate = (boolean === 'true');
        }
        else this.proposalOptions.RoundProposalRate == false;
    },

    initController: function () {
        this.initViews();
        //tbd
        this.viewModel = new ProposalViewModel(this);
        this.view = new ProposalView(this);
        this.view.initView();
        this.$element = $("#proposals_view");
        ko.applyBindings(this.viewModel, this.$element[0]);
    },

    getApiErrorMsg: function (xhr, api, txt) {
        txt = txt || 'Problem with API Request';
        var ret = txt + ' - ' + api + '<br>';
        var message = config.defaultErrorMsg;
        if (xhr.responseJSON) {
            if (xhr.responseJSON.InnerException && xhr.responseJSON.InnerException.ExceptionMessage) {
                message = xhr.responseJSON.InnerException.ExceptionMessage;
            } else {
                if (xhr.responseJSON.ExceptionMessage) {
                    message = xhr.responseJSON.ExceptionMessage;
                }
            }
        } else {
            if (xhr.responseText) message += ('<br>' + xhr.responseText);
            if (xhr.status) message += ' | ' + xhr.status;
        }
        ret += message;
        return ret;
    },

    initViews: function (app) {
        var me = this;
        this.apiLoadInitialData();
    },

    apiLoadInitialData: function () {
        var proposalId = window.location.search.replace('?id=', '');
        var url = baseUrl + '/api/proposal/SalesExplorer/' + proposalId;
        this.proposalId = proposalId;
        httpService.get(url, this.apiLoadInitialDataCompleted.bind(this), null, {
            $ViewElement: $('#proposals_view'),
            ErrorMessage: 'Load Initial Data',
            TitleErrorMessage: 'Error loading initial data',
            StatusMessage: 'Load initial data'
        });
    },

    apiLoadInitialDataCompleted: function (data) {
        var header = data.Header;
        header.HhEqImpressions = data.Totals.HhEqImpressions;
        header.NetCost = data.Totals.NetCost;
        header.GrossCost = data.Totals.GrossCost;
        header.ImsHealthScore = null;
        header.CostPercentage = (header.GrossCost / header.Budget) * 100;

        this.viewModel.Header(data.Header);
        this.view.setGridData(data.Details, data.Totals);
        this.apiLoadIMSDetails(data.Details);
        this.apiFetchNetworks();
    },

    apiLoadIMSDetails: function (details) {
        var url = baseUrl + '/api/proposal/RefreshImsInfoForDetails/' + this.proposalId;
        var data = JSON.stringify(details);

        httpService.post(url, this.apiLoadIMSDetailsCompleted.bind(this), null, data, {
            ErrorMessage: 'Load Initial Data',
            TitleErrorMessage: 'Error loading initial data',
            StatusMessage: 'Load initial data'
        });
    },

    apiLoadIMSDetailsCompleted: function (data) {
        this.view.refreshGridData(data)
    },

    //FETCH NETWORKS
    apiFetchNetworks: function () {
        var url = baseUrl + '/api/proposal/SellableNetworks/' + this.proposalId;

        httpService.get(url, this.apiFetchNetworksCompleted.bind(this), null, {
            //$ViewElement: $('#proposals_view'),
            ErrorMessage: 'Fetch Networks',
            TitleErrorMessage: 'Fetch Networks',
            StatusMessage: 'Fetch Networks'
        });
    },

    apiFetchNetworksCompleted: function (data) {
        this.view.Networks = data.map(function (network) {
            return $.extend({}, network, {
                id: network.Id,
                text: network.Display,
            }); 
        });
    },

    //CPM
    apiChangeHhCpm: function (record, cpmValue) {
        var url = baseUrl + '/api/proposal/ChangeHhCpm/' + this.proposalId;
        var data = JSON.stringify({ eqCpm: cpmValue, detail: record });

        httpService.post(url, this.apiChangeHhCpmCompleted.bind(this, record), null, data, {
            //$ViewElement: $('#proposals_view'),
            ErrorMessage: 'Refreshing IMS Row',
            TitleErrorMessage: 'Error Refreshing IMS Row',
            StatusMessage: 'Refreshing IMS Row'
        });
    },

    apiChangeHhCpmCompleted: function (record, data) {
        var finalData = $.extend({}, data, { recid: record.UniqueId })
        this.view.refreshGridRow(finalData, true)
        this.apiLoadIMSDetail(finalData)
    },

    //DAYPART
    apiChangeDaypart: function (record, newDaypart) {
        if (!newDaypart.Mon && !newDaypart.Tue && !newDaypart.Wed && !newDaypart.Thu && !newDaypart.Fri && !newDaypart.Sat && !newDaypart.Sun)
        {
            return;
        }
        var url = baseUrl + '/api/proposal/ChangeDaypart/' + this.proposalId;
        var data = { detail: record };
        for (var key in newDaypart) {
            if (newDaypart.hasOwnProperty(key)) {
                data[key[0].toLowerCase() + key.substring(1)] = newDaypart[key]
            }
        }

        httpService.post(url, this.apiChangeDaypartCompleted.bind(this, record), null, JSON.stringify(data), {
            //$ViewElement: $('#proposals_view'),
            ErrorMessage: 'Changing Network Daypart',
            TitleErrorMessage: 'Changing Network Daypart',
            StatusMessage: 'Changing Network Daypart'
        });
    },

    apiChangeDaypartCompleted: function (record, data) {
        var finalData = $.extend({}, data, { recid: record.UniqueId })
        this.view.refreshGridRow(finalData, true)
        this.apiLoadIMSDetail(finalData)
    },

    //SPOTS
    apiChangeUnits: function (record, spots) {
        var url = baseUrl + '/api/proposal/ChangeUnits/' + this.proposalId;
        var data = { detail: record, units: spots };

        httpService.post(url, this.apiChangeUnitsCompleted.bind(this, record), null, JSON.stringify(data), {
            //$ViewElement: $('#proposals_view'),
            ErrorMessage: 'Changing Units',
            TitleErrorMessage: 'Changing Units',
            StatusMessage: 'Changing Units'
        });
    },

    apiChangeUnitsCompleted: function (record, data) {
        var finalData = $.extend({}, data, { recid: record.UniqueId })
        //IF COMMENT OUT works after? see bug 72
        this.view.refreshGridRow(finalData, true)
        this.apiLoadIMSDetail(finalData)
    },

    //TBD RATE
    apiChangeRate: function (record, rate) {
        var url = baseUrl + '/api/proposal/ChangeRate/' + this.proposalId;
        var data = {
            detail: record,
            rate: rate,
            planOptions: this.proposalOptions
        };
        httpService.post(url, this.apiChangeRateCompleted.bind(this, record), null, JSON.stringify(data), {
            //$ViewElement: $('#proposals_view'),
            ErrorMessage: 'Change Rate',
            TitleErrorMessage: 'Change Rate',
            StatusMessage: 'Change Rate'
        });
    },

    apiChangeRateCompleted: function (record, data) {
        var finalData = $.extend({}, data, { recid: record.UniqueId })
        this.view.refreshGridRow(finalData, true)
        this.apiLoadIMSDetail(finalData)
    },

    //IMS DETAIL
    apiLoadIMSDetail: function (record) {
        this.view.recordsLoadingIMSData[record.recid] = record;
        var url = baseUrl + '/api/proposal/RefreshImsInfoForDetail/' + this.proposalId;
        var data = JSON.stringify(record);

        httpService.post(url, this.apiLoadIMSDetailCompleted.bind(this, record), null, data, {
            //$ViewElement: $('#proposals_view'),
            ErrorMessage: 'Refreshing IMS Row',
            TitleErrorMessage: 'Error Refreshing IMS Row',
            StatusMessage: 'Refreshing IMS Row'
        });
    },

    apiLoadIMSDetailCompleted: function (record, data) {
        this.view.recordsLoadingIMSData[record.recid] = null;
        var finalData = $.extend({}, data, { recid: record.UniqueId })

        this.view.recordsAlreadyRefreshed[finalData.recid] = finalData;
        this.view.renderRowIMSData(finalData, true);
        this.view.refreshGridRow(finalData, true);
    },

    //TOTALS
    apiLoadRefreshTotalsRow: function (totals, details) {
        var url = baseUrl + '/api/proposal/RefreshSalesExplorerTotals/' + this.proposalId;
        var data = JSON.stringify({ totals: totals, details: details });
        httpService.post(url, this.apiLoadRefreshTotalsRowCompleted.bind(this), null, data, {
            //$ViewElement: $('#proposals_view'),
            ErrorMessage: 'Refreshing IMS Totals',
            TitleErrorMessage: 'Error Refreshing IMS Totals',
            StatusMessage: 'Refreshing IMS Totals'
        });
    },

    apiLoadRefreshTotalsRowCompleted: function (data) {
        this.view.refreshGridTotalsRow(data);
    },

    //ADD NETWORK
    apiAddNetwork: function (networkId) {
        var url = baseUrl + '/api/proposal/AddNetwork/' + this.proposalId;
        //var data = JSON.stringify({ networkId: networkId });
        var data = networkId;
        httpService.post(url, this.apiAddNetworkCompleted.bind(this), null, data, {
            $ViewElement: $('#proposals_view'),
            ErrorMessage: 'Add Network',
            TitleErrorMessage: 'Add Network Error',
            StatusMessage: 'Add Network'
        })
    },

    apiAddNetworkCompleted: function (data) {
        this.view.addRecordToGrid(data);
    },
    //ADD NETWORK
    apiSaveProposal: function (details) {
        var url = baseUrl + '/api/proposal/SaveProposalDetails/' + this.proposalId;
        var data = JSON.stringify(details);

        httpService.post(url, this.apiSaveProposalCompleted.bind(this), null, data, {
            $ViewElement: $('#proposals_view'),
            ErrorMessage: 'Save Proposal',
            TitleErrorMessage: 'Save Proposal Error',
            StatusMessage: 'Save Proposal',
            LoadingMessage: 'Saving...',
        })
    },

    apiSaveProposalCompleted: function (data) {
        console.log(data, 22)
    },
});