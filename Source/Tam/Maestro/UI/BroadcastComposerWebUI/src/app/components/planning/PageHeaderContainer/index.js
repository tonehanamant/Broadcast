import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import { getPlanningFiltered } from 'Ducks/planning';
import { Row, Col, Glyphicon, Button } from 'react-bootstrap';
import { Actions } from 'react-redux-grid';
import SearchInputButton from 'Components/shared/SearchInputButton';

const { MenuActions } = Actions;
const { showMenu, hideMenu } = MenuActions;

const mapStateToProps = ({ routing }) => ({
  routing,
});

const mapDispatchToProps = dispatch => (
  bindActionCreators(
    { getPlanningFiltered,
      showMenu,
      hideMenu,
     }, dispatch)
);

/* eslint-disable react/prefer-stateless-function */
export class PageHeaderContainer extends Component {
  constructor(props) {
    super(props);
    console.log(this.props);
    this.openCreateProposalDetail = this.openCreateProposalDetail.bind(this);
		this.SearchInputAction = this.SearchInputAction.bind(this);
    this.SearchSubmitAction = this.SearchSubmitAction.bind(this);
    this.showHeaderMenu = this.showHeaderMenu.bind(this);
    this.onHideHeaderMenu = this.onHideHeaderMenu.bind(this);
  }

  // see grid - using show/hide column menu actions from here; event listeners to clsoe on click off

  componentDidMount() {
    // document.body.addEventListener('click', this.hideHeaderMenu);
    document.getElementById('planning-section').addEventListener('click', this.onHideHeaderMenu);
  }

  componentWillUnmount() {
    // document.body.removeEventListener('click', this.hideHeaderMenu);
    document.getElementById('planning-section').removeEventListener('click', this.onHideHeaderMenu);
  }

  showHeaderMenu() {
    this.props.showMenu({ id: 'header-row', stateKey: 'gridPlanningHome' });
  }

  onHideHeaderMenu(e) {
    // console.log('hide header', e.target.className, e);
    // click outside of menu - close
    const targ = e.target.className;
    const parent = e.target.parentNode.className;
    if (targ === 'react-grid-action-menu-item' || parent === 'react-grid-action-menu-item') {
      // this.props.hideMenu({ id: 'header-row', stateKey: 'gridPlanningHome' });
    } else {
      this.props.hideMenu({ id: 'header-row', stateKey: 'gridPlanningHome' });
    }
  }

	SearchInputAction() {
		this.props.getPlanningFiltered();
	}

	SearchSubmitAction(value) {
		this.props.getPlanningFiltered(value);
  }

  /* eslint-disable class-methods-use-this */
  openCreateProposalDetail() {
		const url = 'planning/proposal/create';
    window.location.assign(url);
	}

  render() {
    return (
			<Row>
				<Col xs={6}>
          <Button
            bsStyle="success"
            bsSize="small"
            onClick={this.openCreateProposalDetail}
          >Create New Proposal
          </Button>
				</Col>
        <Col xs={6}>
          <Button
            // bsStyle="success"
            style={{ marginLeft: '6px' }}
            bsSize="small"
            className="pull-right"
            onClick={this.showHeaderMenu}
          ><Glyphicon glyph="menu-hamburger" />
          </Button>
					<SearchInputButton
            inputAction={this.SearchInputAction}
            submitAction={this.SearchSubmitAction}
            fieldPlaceHolder="Filter..."
					/>
				</Col>
			</Row>
    );
	}
}

PageHeaderContainer.propTypes = {
  getPlanningFiltered: PropTypes.func.isRequired,
  showMenu: PropTypes.func.isRequired,
  hideMenu: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(PageHeaderContainer);
