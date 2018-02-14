/* eslint-disable no-unused-vars */
import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { Modal, Panel, PanelGroup, Row, Col, Button, ButtonGroup, Form, FormGroup, Glyphicon, Table } from 'react-bootstrap';
import Select from 'react-select';
import { AsyncTypeahead } from 'react-bootstrap-typeahead';
import { bindActionCreators } from 'redux';
import { getGenres, getPrograms } from 'Ducks/planning';

const mapStateToProps = ({ app: { modals: { programGenreModal: modal } }, planning: { genres, isGenresLoading, programs, isProgramsLoading } }) => ({
  modal,
  genres,
  isGenresLoading,
  programs,
  isProgramsLoading,
});

const mapDispatchToProps = dispatch => (
	bindActionCreators({
    getGenres,
    getPrograms,
  }, dispatch)
);

class ProgramGenre extends Component {
  constructor(props) {
    super(props);

    this.handleOnSaveClick = this.handleOnSaveClick.bind(this);
    this.onSave = this.onSave.bind(this);
    this.setCriteriaForSave = this.setCriteriaForSave.bind(this);
    this.readCriteriaFromDetail = this.readCriteriaFromDetail.bind(this);
    this.onCancel = this.onCancel.bind(this);
    this.closeModal = this.closeModal.bind(this);
    this.onProgramSearchSelect = this.onProgramSearchSelect.bind(this);
    this.onGenreSearchSelect = this.onGenreSearchSelect.bind(this);
    this.addIncludeCriteria = this.addIncludeCriteria.bind(this);
    this.removeIncludeCriteria = this.removeIncludeCriteria.bind(this);
    this.addExcludeCriteria = this.addExcludeCriteria.bind(this);
    this.removeExcludeCriteria = this.removeExcludeCriteria.bind(this);
    this.onProgramIncludeClick = this.onProgramIncludeClick.bind(this);
    this.onGenreIncludeClick = this.onGenreIncludeClick.bind(this);
    this.onProgramExcludeClick = this.onProgramExcludeClick.bind(this);
    this.onGenreExcludeClick = this.onGenreExcludeClick.bind(this);

    this.onGenreSearch = this.onGenreSearch.bind(this);
    this.onProgramSearch = this.onProgramSearch.bind(this);
    this.handleProgramPagination = this.handleProgramPagination.bind(this);

    this.setButtonDisabled = this.setButtonDisabled.bind(this);
    this.onModalShow = this.onModalShow.bind(this);
    this.onModalHide = this.onModalHide.bind(this);
    this.resetProgramGenre = this.resetProgramGenre.bind(this);
    this.state = {
      selectedProgram: [],
      selectedGenre: [],
      includeCriteria: [],
      excludeCriteria: [],
      disabledButtons: {
        programInclude: false,
        programExclude: false,
        programAll: true,
        genreInclude: false,
        genreExclude: false,
        genreAll: true,
      },
      programPageSize: 10,
      programResultsLimit: 10,
    };
  }

  // set/reset initial state when modal active
  // todo check detail for existing GenreCriteria, ProgramCriteria
  /* componentWillReceiveProps(nextProps) {
    if (nextProps.modal && nextProps.modal.active && nextProps.detail) {
      this.setState(
        {
          selectedProgram: [],
          selectedGenre: [],
          includeCriteria: [],
          excludeCriteria: [],
          disabledButtons: {
            programInclude: false,
            programExclude: false,
            programAll: true,
            genreInclude: false,
            genreExclude: false,
            genreAll: true,
          },
        },
      );
      this.readCriteriaFromDetail(nextProps.detail);
      console.log('program genre receive props', nextProps, this);
    }
  } */

  resetProgramGenre() {
    this.setState({
      selectedProgram: [],
      selectedGenre: [],
      includeCriteria: [],
      excludeCriteria: [],
      disabledButtons: {
        programInclude: false,
        programExclude: false,
        programAll: true,
        genreInclude: false,
        genreExclude: false,
        genreAll: true,
      },
      programPageSize: 10,
      programResultsLimit: 10,
    });
    this.programTypeahed.getInstance().clear();
    this.genreTypeahed.getInstance().clear();
  }

  handleOnSaveClick() {
    this.onSave();
  }

  onSave() {
    this.setCriteriaForSave();
    this.closeModal();
  }

  // get criteria to update edit form data for BE; break down include/excluded into programs and genres
  // different format for each
  setCriteriaForSave() {
    const programCriteria = [];
    const genreCriteria = [];
    this.state.includeCriteria.forEach((item, idx) => {
      const include = { Contain: 1 };
      if (item.type === 'program') {
        include.Program = { Id: item.Id, Display: item.Display };
        programCriteria.push(include);
      }
      if (item.type === 'genre') {
        include.Genre = { Id: item.Id, Display: item.Display };
        genreCriteria.push(include);
      }
    });
    this.state.excludeCriteria.forEach((item, idx) => {
      const exclude = { Contain: 2 };
      if (item.type === 'program') {
        exclude.Program = { Id: item.Id, Display: item.Display };
        programCriteria.push(exclude);
      }
      if (item.type === 'genre') {
        exclude.Genre = { Id: item.Id, Display: item.Display };
        genreCriteria.push(exclude);
      }
    });
    // console.log('Criteria for save >>>', programCriteria, genreCriteria);
    this.props.updateProposalEditFormDetail({ id: this.props.detail.Id, key: 'ProgramCriteria', value: programCriteria });
    this.props.updateProposalEditFormDetail({ id: this.props.detail.Id, key: 'GenreCriteria', value: genreCriteria });
  }

  // read from existing detail - based on BE format IE {Contain, Program: {Id, Display}} Contain 1 include, Contain 2 exclude
  readCriteriaFromDetail(detail) {
    const programCriteria = [...detail.ProgramCriteria];
    const genreCriteria = [...detail.GenreCriteria];
    programCriteria.forEach((item, idx) => {
      if (item.Contain === 1) {
        this.addIncludeCriteria('program', item.Program);
      }
      if (item.Contain === 2) {
        this.addExcludeCriteria('program', item.Program);
      }
    });
    genreCriteria.forEach((item, idx) => {
      if (item.Contain === 1) {
        this.addIncludeCriteria('genre', item.Genre);
      }
      if (item.Contain === 2) {
        this.addExcludeCriteria('genre', item.Genre);
      }
    });
  }

  onModalShow() {
    // console.log('modal show', this, this.props.detail);
    this.readCriteriaFromDetail(this.props.detail);
  }

  onModalHide() {
    this.resetProgramGenre();
    // console.log('modal hide', this);
  }

  onCancel() {
    this.closeModal();
  }

  closeModal() {
    // this.resetProgramGenre();
    this.props.toggleModal({
      modal: 'programGenreModal',
      active: false,
      properties: { detailId: this.props.detail.Id },
    });
  }

  onProgramSearchSelect(value) {
    // console.log('selected program', value);
    // const val = value ? value.Id : null;
    this.setState({ selectedProgram: value });
    // if value then enable button state all;  else disable
    if (value && value.length) {
      this.setButtonDisabled('programAll', false);
    } else {
      this.setButtonDisabled('programAll', true);
    }
  }

  onGenreSearchSelect(value) {
    // console.log('selected genre', value);
    // const val = value ? value.Id : null;
    this.setState({ selectedGenre: value });
    // if value then enable button state all; else disable
    if (value && value.length) {
      this.setButtonDisabled('genreAll', false);
    } else {
      this.setButtonDisabled('genreAll', true);
    }
  }

  onGenreSearch(query) {
    this.props.getGenres(query);
  }

  onProgramSearch(programQuery) {
    this.setState({ programQuery });
    const params = { Name: programQuery, Start: 1, Limit: this.state.programResultsLimit + 1 };
    this.props.getPrograms(params);
  }

  handleProgramPagination(e) {
    const currentLimit = this.state.programResultsLimit + this.state.programPageSize;
    this.setState({ programResultsLimit: currentLimit });
    const params = { Name: this.state.programQuery, Start: 1, Limit: currentLimit + 1 };
    // this.props.getPrograms(this.state.programQuery, 1, currentLimit + 1);
    this.props.getPrograms(params);
  }

  addIncludeCriteria(type, data) {
    // check already exists; change disabled states
    const dupe = this.state.includeCriteria.find(item => item.Id === data.Id && item.type === type);
    if (!dupe) {
      const key = `${type}_${data.Id}`;
      const item = Object.assign({}, data, { type, key });
      this.state.includeCriteria.push(item);
      const toDisable = (type === 'program') ? 'programExclude' : 'genreExclude';
      this.setButtonDisabled(toDisable, true);
      if (type === 'program') {
        this.setState({ selectedProgram: [] });
        this.programTypeahed.getInstance().clear();
        // disable program selection
        this.setButtonDisabled('programAll', true);
      } else {
      this.setState({ selectedGenre: [] });
      this.genreTypeahed.getInstance().clear();
      // disable genre selection
      this.setButtonDisabled('genreAll', true);
      }
      // console.log('addInclude', this.state, type);
    }
  }

  removeIncludeCriteria(includeItem) {
    const includes = [...this.state.includeCriteria];
    const removed = includes.filter(item => item.key !== includeItem.key);
    this.setState({ includeCriteria: removed });
    // check includes by type to reset enable buttons as needed
    const check = removed.find(item => item.type === includeItem.type);
    if (check === undefined) {
      const toEnable = (includeItem.type === 'program') ? 'programExclude' : 'genreExclude';
      // console.log('toEnable', toEnable);
      this.setButtonDisabled(toEnable, false);
    }
    // console.log('removeIncludeCriteria', check, includeItem, removed, this.state.includeCriteria);
  }

  addExcludeCriteria(type, data) {
    // check already exists; change disabled states; check allowed include/exclude?
    const dupe = this.state.excludeCriteria.find(item => item.Id === data.Id && item.type === type);
    if (!dupe) {
      const key = `${type}_${data.Id}`;
      const item = Object.assign({}, data, { type, key });
      this.state.excludeCriteria.push(item);
      const toDisable = (type === 'program') ? 'programInclude' : 'genreInclude';
      this.setButtonDisabled(toDisable, true);
      if (type === 'program') {
        this.setState({ selectedProgram: [] });
        this.programTypeahed.getInstance().clear();
        // disable program selection
        this.setButtonDisabled('programAll', true);
      } else {
      this.setState({ selectedGenre: [] });
      this.genreTypeahed.getInstance().clear();
      // disable genre selection
      this.setButtonDisabled('genreAll', true);
      }
      // console.log('addExclude', this.state, type);
    }
  }

  removeExcludeCriteria(excludeItem) {
    const excludes = [...this.state.excludeCriteria];
    const removed = excludes.filter(item => item.key !== excludeItem.key);
    this.setState({ excludeCriteria: removed });
    // check excludes by type to reset enable buttons as needed
    const check = removed.find(item => item.type === excludeItem.type);
    if (check === undefined) {
      const toEnable = (excludeItem.type === 'program') ? 'programInclude' : 'genreInclude';
      // console.log('toEnable', toEnable);
      this.setButtonDisabled(toEnable, false);
    }
    console.log('removeIncludeCriteria', excludeItem, removed, this.state.excludeCriteria);
  }

  onProgramIncludeClick() {
    const selected = this.state.selectedProgram;
    if (selected && selected.length) {
      selected.forEach((item, idx) => {
        this.addIncludeCriteria('program', item);
      });
    }
  }

  onProgramExcludeClick() {
    const selected = this.state.selectedProgram;
    if (selected && selected.length) {
      selected.forEach((item, idx) => {
        this.addExcludeCriteria('program', item);
      });
    }
  }

  onGenreIncludeClick() {
    const selected = this.state.selectedGenre;
    if (selected && selected.length) {
      selected.forEach((item, idx) => {
        this.addIncludeCriteria('genre', item);
      });
    }
  }

  onGenreExcludeClick() {
    const selected = this.state.selectedGenre;
    if (selected && selected.length) {
      selected.forEach((item, idx) => {
        this.addExcludeCriteria('genre', item);
      });
    }
  }

  setButtonDisabled(type, disabled) {
    this.setState(prevState => ({
      ...prevState,
      disabledButtons: {
        ...prevState.disabledButtons,
        [type]: disabled,
      },
    }));
  }

  render() {
    const { modal, detail, isReadOnly } = this.props;
    const { selectedProgram, selectedGenre, disabledButtons, includeCriteria, excludeCriteria } = this.state;
    const show = (detail && modal && modal.properties.detailId === detail.Id) ? modal.active : false;

    return (
      <div>
        <Modal show={show} onEntered={this.onModalShow} onExit={this.onModalHide} bsSize="large">
          <Modal.Header>
            <Button className="close" bsStyle="link" onClick={this.onCancel} style={{ display: 'inline-block', float: 'right' }}>
            <span>&times;</span>
          </Button>
          <Modal.Title>
            Include/Exclude Programs/Genres
            {isReadOnly && <span style={{ color: 'red' }}> (Read Only)</span>}
            </Modal.Title>
          </Modal.Header>
          <Modal.Body>
            <Row>
              <Col sm={12}>
                <PanelGroup id="panel_actions_group" style={{ margin: 0 }}>
                  <Panel>
                    <Panel.Heading style={{ padding: '8px 12px' }}>
                      <Row>
                        <Col sm={8}>
                          Program
                        </Col>
                        <Col sm={4}>
                          Include/Exclude
                        </Col>
                      </Row>
                    </Panel.Heading>
                    <Panel.Body>
                      <Row>
                      <Col sm={8} style={{ maxHeight: '48px' }}>
                      <AsyncTypeahead
                        options={this.props.programs}
                        ref={(input) => { this.programTypeahed = input; }}
                        isLoading={this.props.isProgramsLoading}
                        allowNew={false}
                        multiple
                        // cache
                        labelKey="Display"
                        minLength={2}
                        onSearch={this.onProgramSearch}
                        onChange={this.onProgramSearchSelect}
                        placeholder="Search Programs..."
                        disabled={isReadOnly}
                        maxResults={this.state.programPageSize}
                        paginate
                        onPaginate={this.handleProgramPagination}
                      />
                    </Col>
                        <Col sm={4} style={{ maxHeight: '34px' }}>
                          <ButtonGroup justified>
                            <Button disabled={isReadOnly || disabledButtons.programInclude || disabledButtons.programAll} style={{ width: '50%', maxHeight: '34px', paddingTop: '4px' }} onClick={this.onProgramIncludeClick}><Glyphicon className="text-success" style={{ color: '#666', fontSize: '22px' }} glyph="plus-sign" /></Button>
                            <Button disabled={isReadOnly || disabledButtons.programExclude || disabledButtons.programAll} style={{ width: '50%', maxHeight: '34px', paddingTop: '4px' }} onClick={this.onProgramExcludeClick}><Glyphicon className="text-warning" style={{ color: '#666', fontSize: '22px' }} glyph="minus-sign" /></Button>
                          </ButtonGroup>
                        </Col>
                      </Row>
                    </Panel.Body>
                  </Panel>
                  <Panel>
                    <Panel.Heading style={{ padding: '8px 12px' }}>
                      <Row>
                        <Col sm={8}>
                          Genre
                        </Col>
                        <Col sm={4}>
                          Include/Exclude
                        </Col>
                      </Row>
                    </Panel.Heading>
                    <Panel.Body>
                      <Row>
                      <Col sm={8} style={{ maxHeight: '48px' }}>
                      <AsyncTypeahead
                        options={this.props.genres}
                        ref={(input) => { this.genreTypeahed = input; }}
                        isLoading={this.props.isGenresLoading}
                        allowNew={false}
                        multiple
                        labelKey="Display"
                        minLength={2}
                        disabled={isReadOnly}
                        onChange={this.onGenreSearchSelect}
                        onSearch={this.onGenreSearch}
                        placeholder="Search Genres..."
                      />
                    </Col>
                        <Col sm={4} style={{ maxHeight: '34px' }}>
                          <ButtonGroup justified>
                            <Button disabled={isReadOnly || disabledButtons.genreInclude || disabledButtons.genreAll} style={{ width: '50%', maxHeight: '34px', paddingTop: '4px' }} onClick={this.onGenreIncludeClick}><Glyphicon style={{ color: '#666', fontSize: '22px' }} glyph="plus-sign" /></Button>
                            <Button disabled={isReadOnly || disabledButtons.genreExclude || disabledButtons.genreAll} style={{ width: '50%', maxHeight: '34px', paddingTop: '4px' }} onClick={this.onGenreExcludeClick}><Glyphicon style={{ color: '#666', fontSize: '22px' }} glyph="minus-sign" /></Button>
                          </ButtonGroup>
                        </Col>
                      </Row>
                    </Panel.Body>
                  </Panel>
                </PanelGroup>
              </Col>
            </Row>
            <Row>
              <hr />
              <Col md={6}>
                <Panel>
                  <Panel.Heading style={{ padding: '8px 12px' }}>Includes</Panel.Heading>
                  <Panel.Body style={{ padding: 2 }}>
                    <Table responsive condensed>
                      <thead>
                        <tr>
                          <th>Program</th>
                          <th>Genre</th>
                          <th style={{ width: '60px' }}>Action</th>
                        </tr>
                      </thead>
                      <tbody>
                      {includeCriteria.map((item, idx) =>
                      (<tr key={item.key}>
                        {item.type === 'program' &&
                        <td>{item.Display}</td>
                        }
                        {item.type === 'program' &&
                        <td>&nbsp;</td>
                        }
                        {item.type === 'genre' &&
                        <td>&nbsp;</td>
                        }
                        {item.type === 'genre' &&
                        <td>{item.Display}</td>
                        }
                        <td><Button disabled={isReadOnly} onClick={() => this.removeIncludeCriteria(item)} bsStyle="link" style={{ padding: '0 8px' }}><Glyphicon style={{ color: '#c12e2a', fontSize: '12px' }} glyph="trash" /></Button></td>
                      </tr>),
                      )}
                    </tbody>
                    </Table>
                  </Panel.Body>
                </Panel>
              </Col>
              <Col md={6}>
                <Panel>
                  <Panel.Heading style={{ padding: '8px 12px' }}>Excludes</Panel.Heading>
                  <Panel.Body style={{ padding: 2 }}>
                    <Table responsive condensed>
                      <thead>
                        <tr>
                          <th>Program</th>
                          <th>Genre</th>
                          <th style={{ width: '60px' }}>Action</th>
                        </tr>
                      </thead>
                      <tbody>
                      {excludeCriteria.map((item, idx) =>
                        (<tr key={item.key}>
                          {item.type === 'program' &&
                          <td>{item.Display}</td>
                          }
                          {item.type === 'program' &&
                          <td>&nbsp;</td>
                          }
                          {item.type === 'genre' &&
                          <td>&nbsp;</td>
                          }
                          {item.type === 'genre' &&
                          <td>{item.Display}</td>
                          }
                          <td><Button disabled={isReadOnly} onClick={() => this.removeExcludeCriteria(item)} bsStyle="link" style={{ padding: '0 8px' }}><Glyphicon style={{ color: '#c12e2a', fontSize: '12px' }} glyph="trash" /></Button></td>
                        </tr>),
                        )}
                      </tbody>
                    </Table>
                  </Panel.Body>
                </Panel>
              </Col>
            </Row>
          </Modal.Body>

          <Modal.Footer>
            <Button onClick={this.onCancel} bsStyle="default">Cancel</Button>
            {!isReadOnly && <Button onClick={this.handleOnSaveClick} bsStyle="success">OK</Button>}
          </Modal.Footer>
        </Modal>
      </div>
    );
  }
}

ProgramGenre.defaultProps = {
  modal: null,
  updateProposalEditFormDetail: () => {},
  detail: null,
  isReadOnly: false,
};

ProgramGenre.propTypes = {
  modal: PropTypes.object,
  toggleModal: PropTypes.func.isRequired,
  detail: PropTypes.object,
  isReadOnly: PropTypes.bool,
  getGenres: PropTypes.func.isRequired,
  genres: PropTypes.array.isRequired,
  isGenresLoading: PropTypes.bool.isRequired,
  getPrograms: PropTypes.func.isRequired,
  programs: PropTypes.array.isRequired,
  isProgramsLoading: PropTypes.bool.isRequired,
  updateProposalEditFormDetail: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(ProgramGenre);
