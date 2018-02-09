/* eslint-disable no-unused-vars */
import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { Modal, Panel, PanelGroup, Row, Col, Button, ButtonGroup, Form, FormGroup, Glyphicon, Table } from 'react-bootstrap';
import Select from 'react-select';

const mapStateToProps = ({ app: { modals: { programGenreModal: modal } } }) => ({
  modal,
});

class ProgramGenre extends Component {
  constructor(props) {
    super(props);

    this.handleOnSaveClick = this.handleOnSaveClick.bind(this);
    this.onSave = this.onSave.bind(this);
    this.onCancel = this.onCancel.bind(this);
    this.closeModal = this.closeModal.bind(this);
    this.onProgramSearchSelect = this.onProgramSearchSelect.bind(this);
    this.onGenreSearchSelect = this.onGenreSearchSelect.bind(this);

    this.state = {
      selectedProgram: null,
      selectedGenre: null,
      disabledButtons: {
        programInclude: false,
        programExclude: false,
        genreInclude: false,
        genreExclude: false,
      },
    };
  }

  handleOnSaveClick() {
    this.onSave();
  }

  onSave() {
    this.closeModal();
  }

  onCancel() {
    this.closeModal();
  }

  closeModal() {
    this.props.toggleModal({
      modal: 'programGenreModal',
      active: false,
      properties: { detailId: this.props.detail.Id },
    });
  }

  onProgramSearchSelect(value) {
    console.log('selected program', value);
    // const val = value ? value.Id : null;
    this.setState({ selectedProgram: value });
  }

  onGenreSearchSelect(value) {
    console.log('selected genre', value);
    // const val = value ? value.Id : null;
    this.setState({ selectedGenre: value });
  }

  render() {
    const { modal, programsSearchData, genresSearchData, detail, isReadOnly } = this.props;
    const { selectedProgram, selectedGenre, disabledButtons } = this.state;
    const show = (detail && modal && modal.properties.detailId === detail.Id) ? modal.active : false;

    return (
      <div>
        <Modal show={show} bsSize="large">
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
                        <Col sm={8} style={{ maxHeight: '34px' }}>
                          <Form>
                            <FormGroup controlId="programs">
                              <Select
                                value={selectedProgram}
                                onChange={this.onProgramSearchSelect}
                                options={programsSearchData}
                                labelKey="Display"
                                valueKey="Id"
                                clearable={false}
                                disabled={isReadOnly}
                              />
                            </FormGroup>
                          </Form>
                        </Col>
                        <Col sm={4} style={{ maxHeight: '34px' }}>
                          <ButtonGroup justified>
                            <Button disabled={isReadOnly || disabledButtons.programInclude} style={{ width: '50%', maxHeight: '34px', paddingTop: '4px' }}><Glyphicon className="text-success" style={{ color: '#666', fontSize: '22px' }} glyph="plus-sign" /></Button>
                            <Button disabled={isReadOnly || disabledButtons.programExclude} style={{ width: '50%', maxHeight: '34px', paddingTop: '4px' }}><Glyphicon className="text-warning" style={{ color: '#666', fontSize: '22px' }} glyph="minus-sign" /></Button>
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
                        <Col sm={8} style={{ maxHeight: '34px' }}>
                          <Form>
                            <FormGroup controlId="genres">
                              <Select
                                value={selectedGenre}
                                onChange={this.onGenreSearchSelect}
                                options={genresSearchData}
                                labelKey="Display"
                                valueKey="Id"
                                clearable={false}
                                disabled={isReadOnly}
                              />
                            </FormGroup>
                          </Form>
                        </Col>
                        <Col sm={4} style={{ maxHeight: '34px' }}>
                          <ButtonGroup justified>
                            <Button disabled={isReadOnly || disabledButtons.genreInclude} style={{ width: '50%', maxHeight: '34px', paddingTop: '4px' }}><Glyphicon style={{ color: '#666', fontSize: '22px' }} glyph="plus-sign" /></Button>
                            <Button disabled={isReadOnly || disabledButtons.genreExclude} style={{ width: '50%', maxHeight: '34px', paddingTop: '4px' }}><Glyphicon style={{ color: '#666', fontSize: '22px' }} glyph="minus-sign" /></Button>
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
                      <tr>
                        <td>Jimmy Kimmel</td>
                        <td>&nbsp;</td>
                        <td><Button disabled={isReadOnly} bsStyle="link" style={{ padding: '0 8px' }}><Glyphicon style={{ color: '#c12e2a', fontSize: '12px' }} glyph="trash" /></Button></td>
                      </tr>
                      <tr>
                        <td>Seinfeld</td>
                        <td>&nbsp;</td>
                        <td><Button disabled={isReadOnly} bsStyle="link" style={{ padding: '0 8px' }}><Glyphicon style={{ color: '#c12e2a', fontSize: '12px' }} glyph="trash" /></Button></td>
                      </tr>
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
                      <tr>
                        <td>&nbsp;</td>
                        <td>Talk Show</td>
                        <td><Button disabled={isReadOnly} bsStyle="link" style={{ padding: '0 8px' }}><Glyphicon style={{ color: '#c12e2a', fontSize: '12px' }} glyph="trash" /></Button></td>
                      </tr>
                    </tbody>
                  </Table>
                </Panel.Body>
              </Panel>
            </Col>
            </Row>
          </Modal.Body>

          <Modal.Footer>
            <Button onClick={this.onCancel} bsStyle="default">Cancel</Button>
            {!isReadOnly && <Button onClick={this.handleOnSaveClick} bsStyle="success">Save</Button>}
          </Modal.Footer>
        </Modal>
      </div>
    );
  }
}

ProgramGenre.defaultProps = {
  modal: null,
  programsSearchData: [
    {
      Id: 1,
      Display: 'Program 1',
    },
    {
      Id: 2,
      Display: 'Program 2',
    },
    {
      Id: 3,
      Display: 'Program 3',
    },
  ],
  genresSearchData: [
    {
      Id: 1,
      Display: 'Genre 1',
    },
    {
      Id: 2,
      Display: 'Genre 2',
    },
    {
      Id: 3,
      Display: 'Genre 3',
    },
  ],
  detail: null,
  isReadOnly: false,
};

ProgramGenre.propTypes = {
  modal: PropTypes.object,
  toggleModal: PropTypes.func.isRequired,
  genresSearchData: PropTypes.array,
  programsSearchData: PropTypes.array,
  // initialdata: PropTypes.object, // tbd if needed
  detail: PropTypes.object,
  isReadOnly: PropTypes.bool,
};

export default connect(mapStateToProps)(ProgramGenre);
