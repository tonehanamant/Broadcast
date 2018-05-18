import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { Modal, Panel, PanelGroup, Row, Col, Button, ButtonGroup, Glyphicon, Table } from 'react-bootstrap';
import { AsyncTypeahead } from 'react-bootstrap-typeahead';
import { bindActionCreators } from 'redux';
import { getGenres, getPrograms, getShowTypes } from 'Ducks/planning';

const mapStateToProps = ({ app: { modals: { programGenreModal: modal } }, planning: { genres, isGenresLoading, programs, isProgramsLoading, showTypes, isShowTypesLoading } }) => ({
  modal,
  genres,
  isGenresLoading,
  programs,
  isProgramsLoading,
  showTypes,
  isShowTypesLoading,
});

const mapDispatchToProps = dispatch => (
	bindActionCreators({
    getGenres,
    getPrograms,
    getShowTypes,
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
    this.onShowTypeSearchSelect = this.onShowTypeSearchSelect.bind(this);

    this.addIncludeCriteria = this.addIncludeCriteria.bind(this);
    this.removeIncludeCriteria = this.removeIncludeCriteria.bind(this);
    this.addExcludeCriteria = this.addExcludeCriteria.bind(this);
    this.removeExcludeCriteria = this.removeExcludeCriteria.bind(this);

    this.onProgramIncludeClick = this.onProgramIncludeClick.bind(this);
    this.onGenreIncludeClick = this.onGenreIncludeClick.bind(this);
    this.onShowTypeIncludeClick = this.onShowTypeIncludeClick.bind(this);

    this.onProgramExcludeClick = this.onProgramExcludeClick.bind(this);
    this.onGenreExcludeClick = this.onGenreExcludeClick.bind(this);
    this.onShowTypeExcludeClick = this.onShowTypeExcludeClick.bind(this);

    this.onGenreSearch = this.onGenreSearch.bind(this);
    this.onProgramSearch = this.onProgramSearch.bind(this);
    this.onShowTypeSearch = this.onShowTypeSearch.bind(this);

    this.handleProgramPagination = this.handleProgramPagination.bind(this);

    this.setButtonDisabled = this.setButtonDisabled.bind(this);
    this.onModalShow = this.onModalShow.bind(this);
    this.onModalHide = this.onModalHide.bind(this);
    this.resetProgramGenre = this.resetProgramGenre.bind(this);
    this.state = {
      selectedProgram: [],
      selectedGenre: [],
      selectedShowType: [],
      includeCriteria: [],
      excludeCriteria: [],
      disabledButtons: {
        programInclude: false,
        programExclude: false,
        programAll: true,
        genreInclude: false,
        genreExclude: false,
        genreAll: true,
        showTypeInclude: false,
        showTypeExclude: false,
        showTypeAll: true,
      },
      programPageSize: 10,
      programResultsLimit: 10,
    };
  }

  resetProgramGenre() {
    this.setState({
      selectedProgram: [],
      selectedGenre: [],
      selectedShowType: [],
      includeCriteria: [],
      excludeCriteria: [],
      disabledButtons: {
        programInclude: false,
        programExclude: false,
        programAll: true,
        genreInclude: false,
        genreExclude: false,
        genreAll: true,
        showTypeInclude: false,
        showTypeExclude: false,
        showTypeAll: true,
      },
      programPageSize: 10,
      programResultsLimit: 10,
    });
    this.programTypeahed.getInstance().clear();
    this.genreTypeahed.getInstance().clear();
    this.showTypeTypeahed.getInstance().clear();
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
    const showTypeCriteria = [];
    this.state.includeCriteria.forEach((item) => {
      const include = { Contain: 1 };
      if (item.type === 'program') {
        include.Program = { Id: item.Id, Display: item.Display };
        programCriteria.push(include);
      }
      if (item.type === 'genre') {
        include.Genre = { Id: item.Id, Display: item.Display };
        genreCriteria.push(include);
      }
      if (item.type === 'showType') {
        include.ShowType = { Id: item.Id, Display: item.Display };
        showTypeCriteria.push(include);
      }
    });
    this.state.excludeCriteria.forEach((item) => {
      const exclude = { Contain: 2 };
      if (item.type === 'program') {
        exclude.Program = { Id: item.Id, Display: item.Display };
        programCriteria.push(exclude);
      }
      if (item.type === 'genre') {
        exclude.Genre = { Id: item.Id, Display: item.Display };
        genreCriteria.push(exclude);
      }
      if (item.type === 'showType') {
        exclude.ShowType = { Id: item.Id, Display: item.Display };
        showTypeCriteria.push(exclude);
      }
    });
    // console.log('Criteria for save >>>', programCriteria, genreCriteria, showTypeCriteria);
    this.props.updateProposalEditFormDetail({ id: this.props.detail.Id, key: 'ProgramCriteria', value: programCriteria });
    this.props.updateProposalEditFormDetail({ id: this.props.detail.Id, key: 'GenreCriteria', value: genreCriteria });
    this.props.updateProposalEditFormDetail({ id: this.props.detail.Id, key: 'ShowTypeCriteria', value: showTypeCriteria });
  }

  // read from existing detail - based on BE format IE {Contain, Program: {Id, Display}} Contain 1 include, Contain 2 exclude
  readCriteriaFromDetail(detail) {
    const programCriteria = [...detail.ProgramCriteria];
    const genreCriteria = [...detail.GenreCriteria];
    const showTypeCriteria = [...detail.ShowTypeCriteria];
    programCriteria.forEach((item) => {
      if (item.Contain === 1) {
        this.addIncludeCriteria('program', item.Program);
      }
      if (item.Contain === 2) {
        this.addExcludeCriteria('program', item.Program);
      }
    });
    genreCriteria.forEach((item) => {
      if (item.Contain === 1) {
        this.addIncludeCriteria('genre', item.Genre);
      }
      if (item.Contain === 2) {
        this.addExcludeCriteria('genre', item.Genre);
      }
    });
    showTypeCriteria.forEach((item) => {
      if (item.Contain === 1) {
        this.addIncludeCriteria('showType', item.ShowType);
      }
      if (item.Contain === 2) {
        this.addExcludeCriteria('showType', item.ShowType);
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

  onGenreSearchSelect(value) {
    this.setState({ selectedGenre: value });

    if (value && value.length) {
      this.setButtonDisabled('genreAll', false);
    } else {
      this.setButtonDisabled('genreAll', true);
    }
  }

  onGenreSearch(query) {
    this.props.getGenres(query);
  }

  onProgramSearchSelect(value) {
    this.setState({ selectedProgram: value });

    if (value && value.length) {
      this.setButtonDisabled('programAll', false);
    } else {
      this.setButtonDisabled('programAll', true);
    }
  }

  onProgramSearch(programQuery) {
    const params = { Name: programQuery, Start: 1, Limit: this.state.programResultsLimit + 1 };
    this.props.getPrograms(params);
  }

  handleProgramPagination() {
    const currentLimit = this.state.programResultsLimit + this.state.programPageSize;
    this.setState({ programResultsLimit: currentLimit });
    const params = { Name: this.programTypeahed.state.query, Start: 1, Limit: currentLimit + 1 };
    // this.props.getPrograms(this.state.programQuery, 1, currentLimit + 1);
    this.props.getPrograms(params);
  }

  onShowTypeSearchSelect(value) {
    this.setState({ selectedShowType: value });

    if (value && value.length) {
      this.setButtonDisabled('showTypeAll', false);
    } else {
      this.setButtonDisabled('showTypeAll', true);
    }
  }

  onShowTypeSearch(query) {
    this.props.getShowTypes(query);
  }

  // add Include Criteria based on type
  addIncludeCriteria(type, data) {
    // check already exists; change disabled states
    const dupe = this.state.includeCriteria.find(item => item.Id === data.Id && item.type === type);
    if (!dupe) {
      const key = `${type}_${data.Id}`;
      const item = Object.assign({}, data, { type, key });
      this.state.includeCriteria.push(item);
      // const toDisable = (type === 'program') ? 'programExclude' : 'genreExclude';
      let toDisable;
      // this.setButtonDisabled(toDisable, true);
      if (type === 'program') {
        toDisable = 'programExclude';
        this.setState({ selectedProgram: [] });
        this.programTypeahed.getInstance().clear();
        // disable program selection
        this.setButtonDisabled('programAll', true);
      } else if (type === 'genre') {
        toDisable = 'genreExclude';
        this.setState({ selectedGenre: [] });
        this.genreTypeahed.getInstance().clear();
        // disable genre selection
        this.setButtonDisabled('genreAll', true);
      } else if (type === 'showType') {
        toDisable = 'showTypeExclude';
        this.setState({ selectedShowType: [] });
        this.showTypeTypeahed.getInstance().clear();
        // disable genre selection
        this.setButtonDisabled('showTypeAll', true);
      }
      this.setButtonDisabled(toDisable, true);
      // console.log('addInclude', this.state, type);
    }
  }
  // Remove Include Criteria includeItem type, key
  removeIncludeCriteria(includeItem) {
    const includes = [...this.state.includeCriteria];
    const removed = includes.filter(item => item.key !== includeItem.key);
    this.setState({ includeCriteria: removed });
    // check includes by type to reset enable buttons as needed
    const check = removed.find(item => item.type === includeItem.type);
    if (check === undefined) {
      const genreType = (includeItem.type === 'genre') ? 'genreExclude' : 'showTypeExclude';
      const toEnable = (includeItem.type === 'program') ? 'programExclude' : genreType;
      // console.log('toEnable', toEnable);
      this.setButtonDisabled(toEnable, false);
    }
    // console.log('removeIncludeCriteria', check, includeItem, removed, this.state.includeCriteria);
  }
  // add Exclude Criteria based on type
  addExcludeCriteria(type, data) {
    // check already exists; change disabled states; check allowed include/exclude?
    const dupe = this.state.excludeCriteria.find(item => item.Id === data.Id && item.type === type);
    if (!dupe) {
      const key = `${type}_${data.Id}`;
      const item = Object.assign({}, data, { type, key });
      this.state.excludeCriteria.push(item);
      // const toDisable = (type === 'program') ? 'programInclude' : 'genreInclude';
      let toDisable;
      // this.setButtonDisabled(toDisable, true);
      if (type === 'program') {
        toDisable = 'programInclude';
        this.setState({ selectedProgram: [] });
        this.programTypeahed.getInstance().clear();
        // disable program selection
        this.setButtonDisabled('programAll', true);
      } else if (type === 'showType') {
        toDisable = 'showTypeInclude';
        this.setState({ selectedShowType: [] });
        this.showTypeTypeahed.getInstance().clear();
        // disable genre selection
        this.setButtonDisabled('showTypeAll', true);
      }
      this.setButtonDisabled(toDisable, true);
      // console.log('addExclude', this.state, type);
    }
  }
   // Remove Exclude Criteria includeItem type, key
  removeExcludeCriteria(excludeItem) {
    const excludes = [...this.state.excludeCriteria];
    const removed = excludes.filter(item => item.key !== excludeItem.key);
    this.setState({ excludeCriteria: removed });
    // check excludes by type to reset enable buttons as needed
    const check = removed.find(item => item.type === excludeItem.type);
    if (check === undefined) {
      const genreType = (excludeItem.type === 'genre') ? 'genreInclude' : 'showTypeInclude';
      const toEnable = (excludeItem.type === 'program') ? 'programInclude' : genreType;
      // console.log('toEnable', toEnable);
      this.setButtonDisabled(toEnable, false);
    }
    // console.log('removeIncludeCriteria', excludeItem, removed, this.state.excludeCriteria);
  }

  onProgramIncludeClick() {
    const selected = this.state.selectedProgram;
    if (selected && selected.length) {
      selected.forEach((item) => {
        this.addIncludeCriteria('program', item);
      });
    }
  }

  onProgramExcludeClick() {
    const selected = this.state.selectedProgram;
    if (selected && selected.length) {
      selected.forEach((item) => {
        this.addExcludeCriteria('program', item);
      });
    }
  }

  onGenreIncludeClick() {
    const selected = this.state.selectedGenre;
    if (selected && selected.length) {
      selected.forEach((item) => {
        this.addIncludeCriteria('genre', item);
      });
    }
  }

  onGenreExcludeClick() {
    const selected = this.state.selectedGenre;
    if (selected && selected.length) {
      selected.forEach((item) => {
        this.addExcludeCriteria('genre', item);
      });
    }
  }

  onShowTypeIncludeClick() {
    const selected = this.state.selectedShowType;
    if (selected && selected.length) {
      selected.forEach((item) => {
        this.addIncludeCriteria('showType', item);
      });
    }
  }

  onShowTypeExcludeClick() {
    const selected = this.state.selectedShowType;
    if (selected && selected.length) {
      selected.forEach((item) => {
        this.addExcludeCriteria('showType', item);
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
    const { modal, detail, isReadOnly, programs, isProgramsLoading } = this.props;
    const { disabledButtons, includeCriteria, excludeCriteria } = this.state;
    const show = (detail && modal && modal.properties.detailId === detail.Id) ? modal.active : false;

    return (
      <div>
        <Modal show={show} id="program_genre_modal" onEntered={this.onModalShow} onExit={this.onModalHide} dialogClassName="large-80-modal">
          <Modal.Header>
            <Button className="close" bsStyle="link" onClick={this.onCancel} style={{ display: 'inline-block', float: 'right' }}>
            <span>&times;</span>
          </Button>
          <Modal.Title>
            Include/Exclude Programs/Genres/Show Type
            {isReadOnly && <span style={{ color: 'red' }}> (Read Only)</span>}
            </Modal.Title>
          </Modal.Header>
          <Modal.Body>
            <Row>
              <Col sm={12}>
                <PanelGroup id="panel_actions_group" style={{ margin: 0 }}>
                  <Panel>
                    <Panel.Heading style={{ padding: '4px 8px' }}>
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
                        <Col sm={8}>
                          <AsyncTypeahead
                            options={programs}
                            ref={(input) => { this.programTypeahed = input; }}
                            isLoading={isProgramsLoading}
                            allowNew={false}
                            multiple
                            labelKey="Display"
                            minLength={2}
                            disabled={isReadOnly}
                            onChange={this.onProgramSearchSelect}
                            onSearch={this.onProgramSearch}
                            placeholder="Search Programs..."
                            paginate
                            maxResults={this.state.programPageSize}
                            onPaginate={this.handleProgramPagination}
                          />
                        </Col>
                        <Col sm={4} style={{ maxHeight: '34px' }}>
                          <ButtonGroup justified>
                            <Button disabled={isReadOnly || disabledButtons.programInclude || disabledButtons.programAll} style={{ width: '50%', maxHeight: '34px', paddingTop: '4px' }} onClick={this.onProgramIncludeClick}><Glyphicon style={{ color: '#666', fontSize: '22px' }} glyph="plus-sign" /></Button>
                            <Button disabled={isReadOnly || disabledButtons.programExclude || disabledButtons.programAll} style={{ width: '50%', maxHeight: '34px', paddingTop: '4px' }} onClick={this.onProgramExcludeClick}><Glyphicon style={{ color: '#666', fontSize: '22px' }} glyph="minus-sign" /></Button>
                          </ButtonGroup>
                        </Col>
                      </Row>
                    </Panel.Body>
                  </Panel>

                  <Panel>
                    <Panel.Heading style={{ padding: '4px 8px' }}>
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
                      <Col sm={8}>
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
                  <Panel>
                    <Panel.Heading style={{ padding: '4px 8px' }}>
                      <Row>
                        <Col sm={8}>
                          Show Type
                        </Col>
                        <Col sm={4}>
                          Include/Exclude
                        </Col>
                      </Row>
                    </Panel.Heading>
                    <Panel.Body>
                      <Row>
                      <Col sm={8}>
                      <AsyncTypeahead
                        options={this.props.showTypes}
                        ref={(input) => { this.showTypeTypeahed = input; }}
                        isLoading={this.props.isShowTypesLoading}
                        allowNew={false}
                        multiple
                        labelKey="Display"
                        minLength={2}
                        disabled={isReadOnly}
                        onChange={this.onShowTypeSearchSelect}
                        onSearch={this.onShowTypeSearch}
                        placeholder="Search Show Types..."
                      />
                    </Col>
                        <Col sm={4} style={{ maxHeight: '34px' }}>
                          <ButtonGroup justified>
                            <Button disabled={isReadOnly || disabledButtons.showTypeInclude || disabledButtons.showTypeAll} style={{ width: '50%', maxHeight: '34px', paddingTop: '4px' }} onClick={this.onShowTypeIncludeClick}><Glyphicon style={{ color: '#666', fontSize: '22px' }} glyph="plus-sign" /></Button>
                            <Button disabled={isReadOnly || disabledButtons.showTypeExclude || disabledButtons.showTypeAll} style={{ width: '50%', maxHeight: '34px', paddingTop: '4px' }} onClick={this.onShowTypeExcludeClick}><Glyphicon style={{ color: '#666', fontSize: '22px' }} glyph="minus-sign" /></Button>
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
                  <Panel.Heading style={{ padding: '4px 8px' }}>Includes</Panel.Heading>
                  <Panel.Body style={{ padding: 2 }}>
                    <Table responsive condensed>
                      <thead>
                        <tr>
                          <th>Program</th>
                          <th>Genre</th>
                          <th>Show Type</th>
                          <th style={{ width: '60px' }}>Action</th>
                        </tr>
                      </thead>
                      <tbody>
                      {includeCriteria.map(item =>
                      (<tr key={item.key}>
                        {item.type === 'program' &&
                        <td>{item.Display}</td>
                        }
                        {item.type === 'program' &&
                        <td>&nbsp;</td>
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
                        {item.type === 'genre' &&
                        <td>&nbsp;</td>
                        }
                        {item.type === 'showType' &&
                        <td>&nbsp;</td>
                        }
                        {item.type === 'showType' &&
                        <td>&nbsp;</td>
                        }
                        {item.type === 'showType' &&
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
                  <Panel.Heading style={{ padding: '4px 8px' }}>Excludes</Panel.Heading>
                  <Panel.Body style={{ padding: 2 }}>
                    <Table responsive condensed>
                      <thead>
                        <tr>
                          <th>Program</th>
                          <th>Genre</th>
                          <th>Show Type</th>
                          <th style={{ width: '60px' }}>Action</th>
                        </tr>
                      </thead>
                      <tbody>
                      {excludeCriteria.map(item =>
                        (<tr key={item.key}>
                          {item.type === 'program' &&
                          <td>{item.Display}</td>
                          }
                          {item.type === 'program' &&
                          <td>&nbsp;</td>
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
                          {item.type === 'genre' &&
                          <td>&nbsp;</td>
                          }
                          {item.type === 'showType' &&
                          <td>&nbsp;</td>
                          }
                          {item.type === 'showType' &&
                          <td>&nbsp;</td>
                          }
                          {item.type === 'showType' &&
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
  getShowTypes: PropTypes.func.isRequired,
  programs: PropTypes.array.isRequired,
  isProgramsLoading: PropTypes.bool.isRequired,
  showTypes: PropTypes.array.isRequired,
  isShowTypesLoading: PropTypes.bool.isRequired,
  updateProposalEditFormDetail: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(ProgramGenre);
