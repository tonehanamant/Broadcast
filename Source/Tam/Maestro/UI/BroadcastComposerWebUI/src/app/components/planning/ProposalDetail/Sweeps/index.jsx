import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { Modal, Button, Form, FormGroup, ControlLabel, Col } from 'react-bootstrap';
import Select from 'react-select';

const mapStateToProps = ({ app: { modals: { sweepsModal: modal } } }) => ({
  modal,
});

class Sweeps extends Component {
  constructor(props) {
    super(props);

    this.onSave = this.onSave.bind(this);
    this.onCancel = this.onCancel.bind(this);
    this.closeModal = this.closeModal.bind(this);

    this.state = {
      shareBook: null,
      currentShareBook: null,
      shareBookOptions: [],
      hutBook: null,
      currentHutBook: null,
      hutBookOptions: [],
      playbackType: null,
      currentPlaybackType: null,
      playbackTypeOptions: [],
      showConfirmation: false,
    };
  }

  componentWillMount() {
    const { initialdata, detail, updateProposalEditFormDetail } = this.props;

    if (detail) {
      const shareBookId = detail.SharePostingBookId || detail.DefaultPostingBooks.DefaultShareBook.PostingBookId;
      const hutBookId = detail.HutPostingBookId || detail.DefaultPostingBooks.DefaultHutBook.PostingBookId;
      const playbackTypeId = detail.PlaybackType || detail.DefaultPostingBooks.DefaultPlaybackType;

      // select options
      const shareBookOptions = [...initialdata.ForecastDefaults.CrunchedMonths];
      const hutBookOptions = [{ Id: -1, Display: 'Use Share Only' }, ...initialdata.ForecastDefaults.CrunchedMonths];
      const playbackTypeOptions = initialdata.ForecastDefaults.PlaybackTypes;

      // selected option
      const shareBook = shareBookOptions.filter(o => o.Id === shareBookId).shift();
      const hutBook = hutBookOptions.filter(o => o.Id === hutBookId).shift();
      const playbackType = playbackTypeOptions.filter(o => o.Id === playbackTypeId).shift();

      this.setState({
        shareBook,
        currentShareBook: shareBook,
        shareBookOptions,

        hutBook,
        currentHutBook: hutBook,
        hutBookOptions,

        playbackType,
        currentPlaybackType: playbackType,
        playbackTypeOptions,
      });

      // default values
      updateProposalEditFormDetail({ id: detail.Id, key: 'SharePostingBookId', value: shareBookId });
      updateProposalEditFormDetail({ id: detail.Id, key: 'HutPostingBookId', value: hutBookId });
      updateProposalEditFormDetail({ id: detail.Id, key: 'PlaybackType', value: playbackTypeId });
    }
  }

  onSave() {
    const { updateProposalEditFormDetail, detail } = this.props;
    const { currentShareBook, currentHutBook, currentPlaybackType } = this.state;

    updateProposalEditFormDetail({ id: detail.Id, key: 'SharePostingBookId', value: currentShareBook.Id });
    updateProposalEditFormDetail({ id: detail.Id, key: 'HutPostingBookId', value: currentHutBook.Id });
    updateProposalEditFormDetail({ id: detail.Id, key: 'PlaybackType', value: currentPlaybackType.Id });

    this.setState({
      shareBook: currentShareBook,
      hutBook: currentHutBook,
      playbackType: currentPlaybackType,
      showConfirmation: false,
    });

    this.closeModal();
  }

  onCancel() {
    const { shareBook, hutBook, playbackType } = this.state;

    // reset current selected values
    this.setState({
      currentShareBook: shareBook,
      currentHutBook: hutBook,
      currentPlaybackType: playbackType,
    });

    this.closeModal();
  }

  closeModal() {
    this.props.toggleModal({
      modal: 'sweepsModal',
      active: false,
      properties: { detailId: this.props.detail.Id },
    });
  }

  render() {
    const { isReadOnly, modal, detail } = this.props;
    const { currentShareBook, currentHutBook, currentPlaybackType, showConfirmation, shareBookOptions, hutBookOptions, playbackTypeOptions } = this.state;
    const show = (detail && modal && modal.properties.detailId === detail.Id) ? modal.active : false;

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
                    value={currentShareBook}
                    onChange={shareBook => this.setState({ currentShareBook: shareBook })}
                    options={shareBookOptions}
                    labelKey="Display"
                    valueKey="Id"
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
                    value={currentHutBook}
                    onChange={hutBook => this.setState({ currentHutBook: hutBook })}
                    options={hutBookOptions}
                    labelKey="Display"
                    valueKey="Id"
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
                    value={currentPlaybackType}
                    onChange={playbackType => this.setState({ currentPlaybackType: playbackType })}
                    options={playbackTypeOptions}
                    labelKey="Display"
                    valueKey="Id"
                    clearable={false}
                  />
                </Col>
              </FormGroup>
            </Form>
          </Modal.Body>

          <Modal.Footer>
            <Button onClick={this.onCancel} bsStyle="default">Cancel</Button>
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
            <Button onClick={() => this.setState({ showConfirmation: false })} bsStyle="default">Cancel</Button>
            <Button onClick={this.onSave} bsStyle="success">Save</Button>
          </Modal.Footer>
        </Modal>
      </div>
    );
  }
}

Sweeps.propTypes = {
  modal: PropTypes.object,
  toggleModal: PropTypes.func.isRequired,
  isReadOnly: PropTypes.bool,
  initialdata: PropTypes.object,
  detail: PropTypes.object,
  updateProposalEditFormDetail: PropTypes.func.isRequired,
};

Sweeps.defaultProps = {
  modal: null,
  isReadOnly: false,
  initialdata: null,
  detail: null,
};

export default connect(mapStateToProps)(Sweeps);
