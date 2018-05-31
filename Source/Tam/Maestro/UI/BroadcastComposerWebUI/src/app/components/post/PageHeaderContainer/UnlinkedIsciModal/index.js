import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import { Button, Modal, Nav, NavItem } from 'react-bootstrap';
import { Grid, Actions } from 'react-redux-grid';
import { archiveUnlinkedIscis, toggleUnlinkedTab } from 'Ducks/post';
import { generateGridConfig } from './util';


const { MenuActions, SelectionActions } = Actions;
const { showMenu, hideMenu } = MenuActions;
const { selectRow, deselectAll } = SelectionActions;


const mapStateToProps = ({ app: { modals: { postUnlinkedIsciModal: modal } } }) => ({
	modal,
});

const mapDispatchToProps = dispatch => (
  bindActionCreators({
    showMenu,
    hideMenu,
    selectRow,
    deselectAll,
    archiveIscis: archiveUnlinkedIscis,
    toggleTab: toggleUnlinkedTab,
  }, dispatch)
);

export class UnlinkedIsciModal extends Component {
  constructor(props, context) {
		super(props, context);
		this.context = context;
    this.close = this.close.bind(this);
    this.onTabSelect = this.onTabSelect.bind(this);

    this.state = {
      activeTab: 'unlinked',
    };
  }

  close() {
    this.props.toggleModal({
      modal: 'postUnlinkedIsciModal',
      active: false,
      properties: this.props.modal.properties,
    });
    this.setState({ activeTab: 'unlinked' });
  }

  onTabSelect(nextTab) {
    const { activeTab } = this.state;
    const { toggleTab } = this.props;
    if (activeTab !== nextTab) {
      this.setState({ activeTab: nextTab });
      toggleTab(nextTab);
    }
  }

  render() {
    const { modal, unlinkedIscis } = this.props;
    const { activeTab } = this.state;
    const grid = generateGridConfig(this.props, activeTab === 'unlinked');

    return (
      <Modal show={modal.active} onHide={this.close} dialogClassName="large-80-modal">
        <Modal.Header>
          <Modal.Title style={{ display: 'inline-block' }}>Unlinked ISCIs</Modal.Title>
          <Button className="close" bsStyle="link" onClick={this.close} style={{ display: 'inline-block', float: 'right' }}>
            <span>&times;</span>
          </Button>
        </Modal.Header>
        <Modal.Body>
          <Nav style={{ marginBottom: 3 }} bsStyle="tabs" activeKey={activeTab} onSelect={this.onTabSelect}>
              <NavItem eventKey="unlinked">Unlinked ISCIs</NavItem>
              <NavItem eventKey="archived">Archived ISCIs</NavItem>
          </Nav>
					<Grid {...grid} data={unlinkedIscis} store={this.context.store} height={460} />
        </Modal.Body>
        <Modal.Footer>
					<Button onClick={this.close}>Close</Button>
        </Modal.Footer>
      </Modal>
    );
  }
}

UnlinkedIsciModal.defaultProps = {
  modal: {
    active: false,
    properties: {},
  },
};

UnlinkedIsciModal.propTypes = {
	modal: PropTypes.object,
	toggleModal: PropTypes.func.isRequired,
  unlinkedIscis: PropTypes.array.isRequired,
  archiveIscis: PropTypes.array.isRequired,
  toggleTab: PropTypes.func.isRequired,

	deselectAll: PropTypes.func.isRequired,
	selectRow: PropTypes.func.isRequired,
	showMenu: PropTypes.func.isRequired,
	hideMenu: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(UnlinkedIsciModal);
