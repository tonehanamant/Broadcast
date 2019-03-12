import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "react-redux/node_modules/redux";
import { toggleModal, createAlert, setOverlayLoading } from "Main/redux/ducks";
import {
  getPostPrePostingInitialData,
  getPostPrePosting,
  getPostPrePostingFiltered,
  deletePostPrePosting,
  getPostPrePostingFileEdit
} from "PostPrePosting/redux/ducks";
import Table, { withGrid } from "Lib/react-table";

const mapStateToProps = ({ postPrePosting: { initialdata, post }, menu }) => ({
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

export class DataGridContainer extends Component {
  constructor(props, context) {
    super(props, context);
    this.context = context;
    this.contextMenuDeleteAction = this.contextMenuDeleteAction.bind(this);
    this.contextMenuFileSettingsAction = this.contextMenuFileSettingsAction.bind(
      this
    );
  }

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

  render() {
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
        accessor: "UploadDate",
        minWidth: 10,
        Cell: row => <span>{row.original.DisplayUploadDate}</span>
      },
      {
        Header: "Last Modified",
        accessor: "ModifiedDate",
        minWidth: 10,
        Cell: row => <span>{row.original.DisplayModifiedDate}</span>
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
        defaultSorted={[
          {
            id: "ModifiedDate",
            desc: true
          }
        ]}
        selection="single"
      />
    );
  }
}

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
