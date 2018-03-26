import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import CSSModules from 'react-css-modules';
// import { bindActionCreators } from 'redux';
import { Grid } from 'react-redux-grid';
import styles from './index.scss';
// import { Grid, Actions. applyGridConfig } from 'react-redux-grid';
// import Sorter from 'Utils/react-redux-grid-sorter';

// import { setOverlayLoading } from 'Ducks/app';
// import CustomPager from 'Components/shared/CustomPager';
import TestFilterGridCell from './Filters/TestFilterGridCell';

// const { SelectionActions, GridActions } = Actions;

const mapStateToProps = (grid, dataSource) => ({
  grid,
  dataSource,
});

/* const mapDispatchToProps = dispatch => (bindActionCreators(
    {
      doLocalSort,
      setOverlayLoading,
    }, dispatch)
); */

/* eslint-disable */
export class PostScrubbingFilters extends Component {
  constructor(props, context) {
    super(props, context);
    this.context = context;
  }

    /* componentDidMount() {
      applyGridConfig({
        CLASS_NAMES: {
          HEADER: 'hidden',
        },
        CSS_PREFIX: '',
      });
    }
   /*  shouldComponentUpdate(nextProps) {
        return nextProps.ActiveFilters !== this.props.ActiveFilters;
    }

    componentDidUpdate(prevProps) {
        if (prevProps.ActiveFilters !== this.props.ActiveFilters) {
            this.props.setOverlayLoading({
                id: 'PostScrubbingFiltersGrid',
                loading: true,
            });
          // evaluate column sort direction
        setTimeout(() => {
            const cols = this.props.grid.get('PostScrubbingFiltersGrid').get('columns');
            let sortCol = cols.find(x => x.sortDirection);
            if (!sortCol) sortCol = cols.find(x => x.defaultSortDirection);

            if (sortCol) {
                const datasource = this.props.dataSource.get('PostScrubbingFiltersGrid');
                const sorted = Sorter.sortBy(sortCol.dataIndex, sortCol.sortDirection || sortCol.defaultSortDirection, datasource);

                this.props.doLocalSort({
                    data: sorted,
                    stateKey: 'PostScrubbingFiltersGrid',
                });
            }

            this.props.setOverlayLoading({
                id: 'PostScrubbingFiltersGrid',
                loading: false,
            });
        }, 0);
        }
    } */

    /* ////////////////////////////////// */
    /* // GRID ACTION METHOD BINDINGS
    /* ////////////////////////////////// */



    render() {
      const stateKey = 'PostScrubbingFiltersGrid';
      const inactiveFilterStyle = { backgroundColor: '#bfbfbf', minHeight: '20px', maxHeight: '20px', width: '100%' };
      const columns = [
        {
          name: 'Week Start',
          dataIndex: 'WeekStart',
          width: '6%',
          renderer: ({ row }) => {
            return (
              <div style={ inactiveFilterStyle }></div>
            )
          },
        },
        {
          name: 'Date',
          dataIndex: 'TimeAired',
          width: '6%',
          renderer: ({ row }) => {
            return (
              <div style={ inactiveFilterStyle }></div>
            )
          },
        },
        {
          name: 'Time Aired',
          dataIndex: 'MatchTime',
          width: '6%',
          renderer: ({ row }) => {
            return (
              <div style={ inactiveFilterStyle }></div>
            )
          },
        },
        {
          name: 'Day',
          dataIndex: 'DayOfWeek',
          width: '6%',
          renderer: ({ value, row }) => {
            return (
              <TestFilterGridCell />
            )
          },

        },
        {
          name: 'Ad Length',
          dataIndex: 'SpotLength',
          width: '4%',
          renderer: ({ row }) => {
            return (
              <div style={ inactiveFilterStyle }></div>
            )
          },
        },
        {
          name: 'House ISCI',
          dataIndex: 'ISCI',
          width: '10%',
          renderer: ({ row }) => {
            return (
              <div style={ inactiveFilterStyle }></div>
            )
          },
        },
        {
          name: 'Client ISCI',
          dataIndex: 'ClientISCI',
          width: '10%',
          renderer: ({ row }) => {
            return (
              <div style={ inactiveFilterStyle }></div>
            )
          },
        },
        {
          name: 'Program',
          dataIndex: 'ProgramName',
          width: '12%',
          renderer: ({ row }) => {
            return (
              <div style={ inactiveFilterStyle }></div>
            )
          },
        },
        {
          name: 'Genre',
          dataIndex: 'GenreName',
          width: '6%',
          renderer: ({ row }) => {
            return (
              <div style={ inactiveFilterStyle }></div>
            )
          },
        },
        {
          name: 'Affiliate',
          dataIndex: 'Affiliate',
          width: '6%',
          renderer: ({ row }) => {
            return (
              <div style={ inactiveFilterStyle }></div>
            )
          },
        },
        {
          name: 'Market',
          dataIndex: 'Market',
          width: '12%',
          renderer: ({ row }) => {
            return (
              <div style={ inactiveFilterStyle }></div>
            )
          },
        },
        {
          name: 'Station',
          dataIndex: 'Station',
          width: '6%',
          renderer: ({ row }) => {
            return (
              <div style={ inactiveFilterStyle }></div>
            )
          },
        },
        {
          name: 'Comments',
          dataIndex: 'Comments',
          width: '10%',
          renderer: ({ row }) => {
            return (
              <div style={ inactiveFilterStyle }></div>
            )
          },
        },
    ];

    const plugins = {
      COLUMN_MANAGER: {
        resizable: false,
        moveable: false,
        sortable: {
            enabled: false,
            method: 'local',
        },
      },
    };

    const grid = {
      columns,
      plugins,
      stateKey,
    };

    return (
      <div style={ {marginBottom: '2px' } }>
        <Grid {...grid} classNames={['filter-grid']} data={this.props.ActiveFilters} store={this.context.store} height={false}/>
      </div>
    );
  }
}


PostScrubbingFilters.defaultProps = {
  ActiveFilters: [{DayOfWeek: 'Day Filter'}],
};
PostScrubbingFilters.PropTypes = {
  grid: PropTypes.object.isRequired,
  dataSource: PropTypes.object.isRequired,
  ActiveFilters: PropTypes.array.isRequired,
  // doLocalSort: PropTypes.func.isRequired,
};

const styledComponent = CSSModules(PostScrubbingFilters, styles);
export default connect(mapStateToProps)(PostScrubbingFilters);
