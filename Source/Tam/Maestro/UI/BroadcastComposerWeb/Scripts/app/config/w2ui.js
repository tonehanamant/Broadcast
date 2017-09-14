//globally used renderers (for grids, etc)
var w2uiConfig = {

    renderers: {
        //returns val converted to money number format else dash if not value; option to allow 0 as value
        toMoneyOrDash: function (val, allowZero) {
            //test for val then allow zero test
            // return (val || (allowZero ? (val == 0) : false)) ? ('$' + (parseFloat(Math.round(val * 100) / 100).toFixed(2))) : '-';
            return (val || (allowZero ? (val == 0) : false)) ? ('$' + (w2utils.formatNumber(val.toFixed(2)))) : '-';

        },

        //returns val converted to number format else dash if not value; option to allow 0 as value
        toValueOrDash: function (val, allowZero) {
            //return val ? (parseFloat(Math.round(val * 100) / 100).toFixed(2)) : '-';
            //test for val then allow zero test
            return (val || (allowZero ? (val == 0) : false)) ? (parseFloat(Math.round(val * 100) / 100).toFixed(2)) : '-';
        }
    }

}