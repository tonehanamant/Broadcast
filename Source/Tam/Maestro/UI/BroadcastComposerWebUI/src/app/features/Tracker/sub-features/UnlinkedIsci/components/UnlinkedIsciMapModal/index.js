import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { Button, Modal } from "react-bootstrap";
import { AsyncTypeahead } from "react-bootstrap-typeahead";
import { head } from "lodash";
import {
  mapUnlinkedIscis,
  loadValidIscis,
  mapUnlinkedIsci
} from "Tracker/redux/actions";
import { toggleModal } from "Main/redux/actions";

import "./index.style.scss";

const mapStateToProps = ({
  app: {
    modals: { mapUnlinkedIsci: modal }
  },
  tracker: { loadingValidIscis, typeaheadIscisList }
}) => ({
  modal,
  typeaheadIscisList,
  loadingValidIscis
});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      mapIscis: mapUnlinkedIscis,
      toggleModal,
      loadValidIscis,
      mapUnlinkedIsci
    },
    dispatch
  );

export class UnlinkedIsciMapModal extends Component {
  constructor(props, context) {
    super(props, context);
    this.context = context;
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
    this.props.toggleModal({
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
    const { modal } = this.props;
    const { selectedIsci } = this.state;
    const rowData = modal.properties.rowData || {};
    this.props.mapUnlinkedIsci({
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
    const rowData = modal.properties.rowData;

    return (
      <Modal
        show={modal.active}
        onHide={this.close}
        className="map-unlinked-isci-modal"
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
          <div className="mapping-section">
            <div>
              <span>{`Mapping: ${rowData.ISCI}`}</span>
            </div>
            <div className="typeahead-wrapper">
              <span>To:</span>
              <AsyncTypeahead
                options={typeaheadIscisList}
                isLoading={loadingValidIscis}
                allowNew={false}
                minLength={2}
                onSearch={loadValidIscis}
                onChange={this.onChange}
                placeholder="Search valid ISCI..."
                className="typeahead-element"
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
