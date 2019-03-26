import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { Button, Modal } from "react-bootstrap";
import { AsyncTypeahead } from "react-bootstrap-typeahead";
import { head } from "lodash";
import { unlinkedIsciActions } from "Tracker";
import { toggleModal } from "Main/redux/ducks";

import "./index.style.scss";

const mapStateToProps = ({
  app: {
    modals: { mapUnlinkedIsci: modal }
  },
  tracker: {
    unlinkedIsci: { loadingValidIscis, typeaheadIscisList }
  }
}) => ({
  modal,
  typeaheadIscisList,
  loadingValidIscis
});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      mapIscis: unlinkedIsciActions.mapUnlinkedIscis,
      loadValidIscis: unlinkedIsciActions.loadValidIscis,
      mapUnlinkedIsci: unlinkedIsciActions.mapUnlinkedIsci,
      toggleModal
    },
    dispatch
  );

export class UnlinkedIsciMapModal extends Component {
  constructor(props) {
    super(props);
    this.onCancel = this.onCancel.bind(this);
    this.onSubmit = this.onSubmit.bind(this);
    this.onChange = this.onChange.bind(this);
    this.onClearState = this.onClearState.bind(this);
    this.state = {
      selectedIsci: null
    };
  }

  componentWillReceiveProps(nextProps) {
    const { modal } = this.props;

    if (modal.active && !nextProps.modal.active) {
      this.onClearState();
    }
  }

  onClearState() {
    this.setState({
      selectedIsci: null
    });
  }

  onCancel() {
    const { toggleModal } = this.props;
    toggleModal({
      modal: "mapUnlinkedIsci",
      active: false,
      properties: {}
    });
  }

  onChange(iscis) {
    const isci = head(iscis);
    this.setState({ selectedIsci: isci });
  }

  onSubmit() {
    const { modal, mapUnlinkedIsci } = this.props;
    const { selectedIsci } = this.state;
    const rowData = modal.properties.rowData || {};
    mapUnlinkedIsci({
      OriginalIsci: rowData.ISCI,
      EffectiveIsci: selectedIsci
    });
  }

  render() {
    const { selectedIsci } = this.state;
    const {
      modal,
      loadValidIscis,
      typeaheadIscisList,
      loadingValidIscis
    } = this.props;

    if (!(modal.properties && modal.properties.rowData)) {
      return null;
    }
    const { rowData } = modal.properties;

    return (
      <Modal
        show={modal.active}
        onHide={this.close}
        styleName="map-unlinked-isci-modal"
        dialogClassName="large-40-modal"
      >
        <Modal.Header>
          <Modal.Title style={{ display: "inline-block" }}>
            Map ISCIs
          </Modal.Title>
          <Button
            className="close"
            bsStyle="link"
            onClick={this.close}
            style={{ display: "inline-block", float: "right" }}
          >
            <span>&times;</span>
          </Button>
        </Modal.Header>
        <Modal.Body>
          <div styleName="mapping-section">
            <div>
              <span>{`Mapping: ${rowData.ISCI}`}</span>
            </div>
            <div styleName="typeahead-wrapper">
              <span>To:</span>
              <AsyncTypeahead
                options={typeaheadIscisList}
                isLoading={loadingValidIscis}
                allowNew={false}
                minLength={2}
                onSearch={loadValidIscis}
                onChange={this.onChange}
                placeholder="Search valid ISCI..."
                styleName="typeahead-element"
              />
            </div>
          </div>
        </Modal.Body>
        <Modal.Footer>
          <Button bsStyle="danger" onClick={this.onCancel}>
            Cancel
          </Button>
          <Button
            bsStyle="success"
            onClick={this.onSubmit}
            disabled={!selectedIsci}
          >
            Ok
          </Button>
        </Modal.Footer>
      </Modal>
    );
  }
}

UnlinkedIsciMapModal.defaultProps = {
  modal: {},
  typeaheadIscisList: []
};

UnlinkedIsciMapModal.propTypes = {
  modal: PropTypes.object,
  toggleModal: PropTypes.func.isRequired,
  loadValidIscis: PropTypes.func.isRequired,
  mapUnlinkedIsci: PropTypes.func.isRequired,
  typeaheadIscisList: PropTypes.arrayOf(PropTypes.string),
  loadingValidIscis: PropTypes.bool.isRequired
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(UnlinkedIsciMapModal);