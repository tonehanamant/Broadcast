import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { toggleModal, createAlert, setOverlayLoading } from "Ducks/app";
import {
  getPostPrePostingInitialData,
  getPostPrePosting,
  getPostPrePostingFiltered,
  deletePostPrePosting,
  getPostPrePostingFileEdit
} from "Ducks/postPrePosting";
import Table, { withGrid } from "Lib/react-table";

/* ////////////////////////////////// */
/* // MAPPING STATE AND DISPATCH
/* ////////////////////////////////// */
const mapStateToProps = ({
  postPrePosting: { initialdata },
  postPrePosting: { post },
  menu
}) => ({
  initialdata,
  post,
  menu
});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      getPostPrePostingInitialData,
      getPostPrePosting,
      getPostPrePostingFiltered,
      createAlert,
      toggleModal,
      deletePostPrePosting,
      getPostPrePostingFileEdit,
      setOverlayLoading
    },
    dispatch
  );

/* ////////////////////////////////// */
/* // DATAGRIDCONTAINER COMPONENT
/* ////////////////////////////////// */

export class DataGridContainer extends Component {
  constructor(props, context) {
    super(props, context);
    this.context = context;
    this.contextMenuDeleteAction = this.contextMenuDeleteAction.bind(this);
    this.contextMenuFileSettingsAction = this.contextMenuFileSettingsAction.bind(
      this
    );
  }

  /* ////////////////////////////////// */
  /* GRID CONTEXT MENU METHODS  */
  /* ////////////////////////////////// */
  contextMenuDeleteAction(rowData) {
    this.props.toggleModal({
      modal: "confirmModal",
      active: true,
      properties: {
        titleText: "Delete Post File",
        bodyText: `Are you sure you want to delete ${rowData.FileName}?`,
        closeButtonText: "Cancel",
        actionButtonText: "Continue",
        actionButtonBsStyle: "danger",
        action: () => this.props.deletePostPrePosting(rowData.Id),
        dismiss: () => {}
      }
    });
  }

  contextMenuFileSettingsAction(id) {
    this.props.getPostPrePostingFileEdit(id);
  }

  /* ////////////////////////////////// */
  /* // COMPONENT RENDER FUNC
  /* ////////////////////////////////// */
  render() {
    /* ////////////////////////////////// */
    /* // REACT-TABLE CONFIGURATION
    /* ////////////////////////////////// */

    /* GRID COLUMNS */
    const columns = [
      {
        Header: "File Name",
        accessor: "FileName",
        minWidth: 20
      },
      {
        Header: "Demos",
        accessor: "DisplayDemos",
        minWidth: 20
      },
      {
        Header: "Upload Date",
        accessor: "DisplayUploadDate",
        minWidth: 10,
        Cell: row => <span>{row.value}</span>
      },
      {
        Header: "Last Modified",
        accessor: "DisplayModifiedDate",
        minWidth: 10,
        Cell: row => <span>{row.value}</span>
      }
    ];

    const menuItems = [
      {
        text: "File Settings",
        key: "menu-file-settings",
        EVENT_HANDLER: ({ metaData }) => {
          this.contextMenuFileSettingsAction(metaData.rowData.Id);
        }
      },
      {
        text: "Post Report",
        key: "menu-post-report",
        EVENT_HANDLER: ({
          metaData: {
            rowData: { Id }
          }
        }) => {
          window.open(`${__API__}PostPrePosting/Report/${Id}`, "_blank");
        }
      },
      {
        text: "Delete",
        key: "menu-delete",
        EVENT_HANDLER: ({ metaData }) => {
          this.contextMenuDeleteAction(metaData.rowData);
        }
      }
    ];

    return (
      <Table
        data={this.props.post}
        style={{ marginBottom: "100px" }}
        columns={columns}
        contextMenu={{
          isRender: true,
          menuItems
        }}
        selection="single"
      />
    );
  }
}

/* ////////////////////////////////// */
/* // PROPTYPES
/* ////////////////////////////////// */
DataGridContainer.propTypes = {
  post: PropTypes.array.isRequired,
  toggleModal: PropTypes.func.isRequired,
  deletePostPrePosting: PropTypes.func.isRequired,
  getPostPrePostingFileEdit: PropTypes.func.isRequired
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(withGrid(DataGridContainer));
