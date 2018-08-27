import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import { createAlert, toggleModal } from 'Ducks/app';
import { getPostFiltered, getUnlinkedIscis } from 'Ducks/post';
import { Row, Col, Button } from 'react-bootstrap';
import SearchInputButton from 'Components/shared/SearchInputButton';
import UnlinkedIsciModal from './UnlinkedIsciModal';

const mapStateToProps = ({ post: { unlinkedIscis, unlinkedIscisLength } }) => ({
  unlinkedIscis,
  unlinkedIscisLength,
});

const mapDispatchToProps = dispatch => (
  bindActionCreators({ createAlert, getPostFiltered, getUnlinkedIscis, toggleModal }, dispatch)
);

export class PageHeaderContainer extends Component {
  constructor(props) {
		super(props);
		this.SearchInputAction = this.SearchInputAction.bind(this);
    this.SearchSubmitAction = this.SearchSubmitAction.bind(this);
    this.openUnlinkedIscis = this.openUnlinkedIscis.bind(this);
	}

	SearchInputAction() {
		this.props.getPostFiltered();
	}

	SearchSubmitAction(value) {
		this.props.getPostFiltered(value);
  }

  openUnlinkedIscis() {
		this.props.getUnlinkedIscis();
	}

  render() {
    const { unlinkedIscisLength } = this.props;
    return (
      <div>
			<Row>
				<Col xs={6}>
        {!!unlinkedIscisLength &&
          <Button
            bsStyle="success"
            onClick={this.openUnlinkedIscis}
            bsSize="small"
          >
            {`Unlinked ISCIs (${unlinkedIscisLength})`}
          </Button>}
				</Col>
        <Col xs={6}>
					<SearchInputButton
            inputAction={this.SearchInputAction}
            submitAction={this.SearchSubmitAction}
            fieldPlaceHolder="Search..."
					/>
				</Col>
			</Row>
      <UnlinkedIsciModal
        toggleModal={this.props.toggleModal}
        unlinkedIscis={this.props.unlinkedIscis}
      />
    </div>
    );
	}
}

PageHeaderContainer.propTypes = {
  getPostFiltered: PropTypes.func.isRequired,
  getUnlinkedIscis: PropTypes.func.isRequired,
  toggleModal: PropTypes.func.isRequired,
	unlinkedIscis: PropTypes.array.isRequired,
	unlinkedIscisLength: PropTypes.number.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(PageHeaderContainer);
