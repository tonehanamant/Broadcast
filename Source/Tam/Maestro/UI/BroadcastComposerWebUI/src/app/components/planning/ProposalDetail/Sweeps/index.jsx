import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { Modal, Button, Form, FormGroup, ControlLabel, Col } from 'react-bootstrap';
import Select from 'react-select';

class Sweeps extends Component {
  constructor(props) {
    super(props);

    this.onSave = this.onSave.bind(this);
    this.onCancel = this.onCancel.bind(this);

    this.state = {
      shareBook: null,
      hutBook: null,
      playbackType: null,
      showConfirmation: false,
    };
  }

  componentWillMount() {
    const { shareBook, hutBook, playbackType } = this.props;
    this.setState({ shareBook, hutBook, playbackType });
  }

  onSave() {
    const { onSave } = this.props;
    const { shareBook, hutBook, playbackType } = this.state;

    this.setState({ showConfirmation: false });
    onSave(shareBook, hutBook, playbackType);
  }

  onCancel() {
    const { shareBook, hutBook, playbackType, onClose } = this.props;
    this.setState({ shareBook, hutBook, playbackType });
    onClose();
  }

  render() {
    const { show, shareBookOptions, hutBookOptions, playbackTypeOptions, isReadOnly } = this.props;
    const { shareBook, hutBook, playbackType, showConfirmation } = this.state;

    return (
      <div>
        <Modal show={show}>
          <Modal.Header closeButton>
            <Modal.Title>Manage Ratings</Modal.Title>
          </Modal.Header>

          <Modal.Body>
            <Form horizontal>
              <FormGroup controlId="shareBook">
                <Col componentClass={ControlLabel} sm={3}>
                  Share Book
                </Col>
                <Col sm={9}>
                  <Select
                    value={shareBook}
                    onChange={shareBook => this.setState({ shareBook })}
                    options={shareBookOptions}
                    labelKey="Display"
                    clearable={false}
                  />
                </Col>
              </FormGroup>

              <FormGroup controlId="hutBook">
                <Col componentClass={ControlLabel} sm={3}>
                  Hut Book
                </Col>
                <Col sm={9}>
                  <Select
                    value={hutBook}
                    onChange={hutBook => this.setState({ hutBook })}
                    options={hutBookOptions}
                    labelKey="Display"
                    clearable={false}
                  />
                </Col>
              </FormGroup>

              <FormGroup controlId="playbackType">
                <Col componentClass={ControlLabel} sm={3}>
                  Playback Type
                </Col>
                <Col sm={9}>
                  <Select
                    value={playbackType}
                    onChange={playbackType => this.setState({ playbackType })}
                    options={playbackTypeOptions}
                    labelKey="Display"
                    clearable={false}
                  />
                </Col>
              </FormGroup>
            </Form>
          </Modal.Body>

          <Modal.Footer>
            <Button onClick={this.onCancel} bsStyle="danger">Cancel</Button>
            {!isReadOnly && <Button onClick={() => this.setState({ showConfirmation: true })} bsStyle="success">Save</Button>}
          </Modal.Footer>
        </Modal>

        <Modal show={showConfirmation}>
          <Modal.Header closeButton>
            <Modal.Title>Are you sure?</Modal.Title>
          </Modal.Header>

          <Modal.Body>
            Changes to rating books may effect Impressions for existing inventory
          </Modal.Body>

          <Modal.Footer>
            <Button onClick={() => this.setState({ showConfirmation: false })} bsStyle="danger">Cancel</Button>
            <Button onClick={this.onSave} bsStyle="success">Save</Button>
          </Modal.Footer>
        </Modal>
      </div>
    );
  }
}

const optionShape = PropTypes.shape({
  Id: PropTypes.number,
  Display: PropTypes.string,
});

Sweeps.propTypes = {
  show: PropTypes.bool.isRequired,
  onClose: PropTypes.func.isRequired,
  onSave: PropTypes.func.isRequired,
  isReadOnly: PropTypes.bool,

  shareBookOptions: PropTypes.arrayOf(optionShape).isRequired,
  hutBookOptions: PropTypes.arrayOf(optionShape).isRequired,
  playbackTypeOptions: PropTypes.arrayOf(optionShape).isRequired,

  shareBook: optionShape,
  hutBook: optionShape,
  playbackType: optionShape,
};

Sweeps.defaultProps = {
  shareBook: null,
  hutBook: null,
  playbackType: null,
  isReadOnly: false,
};

export default Sweeps;
