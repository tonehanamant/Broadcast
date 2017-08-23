//Manage Ratings Books (TrackerMainView context) - Modal with RatingsBooks Grid - inline editing/delete

var TrackerManageRatingsBooks = function (view) {
    //private
    var _view = view;//TrackerMainView

    return {

        $Modal: null,
        $RatingsBooksGrid: null,
        $AddBookSelect: null,

        activeRatingsBooks: null, //??

        postingBooks: null, //select 2 converted

        newRecordIdCount: 0,

        initView: function () {
            this.$RatingsBooksGrid = $('#ratings_books_grid').w2grid(TrackerConfig.getRatingsBooksGridCfg(this));

            $("#manage_ratings_books_add_book_btn").on('click', this.openBookSelect.bind(this));

            $("#manage_ratings_books_save_btn").on('click', this.onBooksSave.bind(this));

            this.setGridEventHandlers();
            var me = this;

        },

        //set rating data; modal
        setRatings: function (ratingsData) {
            this.activeRatingsBooks = ratingsData.RatingAdjustments;
            this.postingBooks = ratingsData.PostingBooks;
            var me = this;
            if (!this.$Modal) {
                this.$Modal = $('#manage_ratings_books_modal');
                this.$Modal.on('shown.bs.modal', function (event) {
                    me.onSetRatings();
                });

                this.$Modal.modal({
                    backdrop: 'static',
                    show: false,
                    keyboard: false
                });
            }

            this.$Modal.modal('show');
        },

        setGridEventHandlers: function () {
            var me = this;

            //grid clicks - edit
            this.$RatingsBooksGrid.on('click', this.onRatingsGridClick.bind(this));
            //record record changes
            this.$RatingsBooksGrid.on('change', this.onRatingsGridChange.bind(this));

            //menu, delete
            this.$RatingsBooksGrid.on('menuClick', function (event) {
                if ((event.recid == 'addNew')) {
                    event.preventDefault();
                    return false;
                }
                if (event.menuItem.id === 1) { //delete
                    me.handleRatingsDelete(me.$RatingsBooksGrid.get(event.recid));
                }
            });

            this.$RatingsBooksGrid.on('contextMenu', function (event) {
                if ((event.recid == 'addNew')) {
                    event.preventDefault();
                    return false;
                }
            });

        },

        //after modal shown - set grid
        onSetRatings: function () {
            this.setRatingsGrid();
        },

        //prepare data for grid with stored originals; if new record then do not set by Id
        prepareGridData: function (data, isNew) {
            //copy or mutates
            var displayData = isNew ? [data] : util.copyData(data);
            var ret = [];
            var me = this;
            $.each(displayData, function (index, item) {
                item.stored = util.copyData(item);//stored data for BE with changes
                item.recid = isNew ? ('new_' + me.newRecordIdCount++) : item.MediaMonthId;
                item.isNew = isNew ? true : false;//needed?
                ret.push(item);
            });

            return isNew ? ret[0] : ret;
        },

        // set Ratings Books Grid
        setRatingsGrid: function () {
            var ratingsData = this.prepareGridData(this.activeRatingsBooks);
            this.$RatingsBooksGrid.clear(false);
            this.$RatingsBooksGrid.add(ratingsData);
            this.$RatingsBooksGrid.resize();
            this.setBookSelect(true);
        },

        addNewRating: function (bookSelection) {
            var item = {
                MediaMonthId: parseInt(bookSelection.id),
                MediaMonthDisplay: bookSelection.text,
                AnnualAdjustment: 5, //set default
                NtiAdjustment: 20 //set default
            };

            //remove existing then reset
            var newRec = this.prepareGridData(item, true);
            this.$RatingsBooksGrid.remove('addNew');

            this.$RatingsBooksGrid.add(newRec);
            this.setBookSelect(true);
            this.$RatingsBooksGrid.editField(newRec.recid, 1);

        },

        //delete a book (local delete) - confirm?
        handleRatingsDelete: function (rec) {
            //confirm the delete?
            var title = 'Delete Ratings Book',
                message = 'Are you sure you want to delete: ' + rec.MediaMonthDisplay + '?';
            var callback = this.onRatingsDelete.bind(this, rec.recid);
            util.confirm(title, message, callback);
        },

        onRatingsDelete: function (id) {
            this.$RatingsBooksGrid.remove(id);
            //TODO need to add the week back to the active Weeks available
            util.notify('Rating Book Deleted. You must Save Ratings Edits to persist the changes.');
            this.setBookSelect(false);
        },

        //set select 2 based on availabkle items; addNew adds the record first
        setBookSelect: function (addNew) {
            if (addNew) {
                this.$RatingsBooksGrid.add({ recid: 'addNew', addNew: true, w2ui: { editable: false } });
            }
            var months = this.postingBooks;
            //adds placeholder
            var ret = [{ id: '-1', text: 'Select Book' }];
            var me = this;
            //determine slect2 data based on unused items; change to select 2 type array
            $.each(months, function (index, book) {
                var recs = me.$RatingsBooksGrid.find({ MediaMonthId: book.Id });
                if (!recs.length) {
                    ret.push({ id: book.Id, text: book.Display });
                }
            });
            $("#add_book_select").select2({
                //placeholder: 'Select Book',
                placeholder: {
                    id: '-1', // the value of the option
                    text: 'Select Book'
                },
                //theme: 'classic',
                //width: "style"//this no longer works it appears
                data: ret
            });

            $("#add_book_select").on('select2:select', function (evt) {
                console.log('add book select', evt.params.data);
                me.addNewRating(evt.params.data);
                //id is string so parseInt
            });

        },

        //closeBook - need to close programatically?

        //open drop down programatically (add button)
        openBookSelect: function () {
            //possibly scroll to record?
            $("#add_book_select").select2('open');
        },

        //on grid click per column
        onRatingsGridClick: function (event) {
            var record = this.$RatingsBooksGrid.get(event.recid);
            //if (event.column === null || this.readOnly || this.activeEditingRecord) {
            if (event.column === null || record.addNew) {
                return;
            }

            if (event.column == 1) {//YearOnYearLoss - set editing
                this.$RatingsBooksGrid.editField(event.recid, 1);

            } else if (event.column == 2) {//Conversion
                this.$RatingsBooksGrid.editField(event.recid, 2);

            } else {
                //console.log(this.$RatingsBooksGrid.getChanges());
                return;
            }
        },

        //on inline field change - change the value of the record field (W2ui does not update automatically); mark hasChange
        //problem if SET value then is not marked dirty in UI; used stored instead; need to handle null changes
        onRatingsGridChange: function (event) {
            var isYearOnYearLoss = event.column == 1, isConversion = event.column == 2;
            var record = this.$RatingsBooksGrid.get(event.recid);
            //value_new can be set to empty - intercept and change to previous
            if (event.value_new < -100) {
                //do before on complete
                if (isYearOnYearLoss) this.$RatingsBooksGrid.set(event.recid, { AnnualAdjustment: event.value_previous });
                if (isConversion) this.$RatingsBooksGrid.set(event.recid, { NtiAdjustment: event.value_previous });
                //the changes are not updated at this stage but leaves the edit state empty if user sets empty
                //wait for on complete else changes does not exist
                event.onComplete = function () {
                    if (isYearOnYearLoss) record.w2ui.changes.AnnualAdjustment = event.value_previous;

                    if (isConversion) record.w2ui.changes.NtiAdjustment = event.value_previous;

                }
                return;
            }
            var changed = false;
            //value_new, value_previous, value_original
            if (isYearOnYearLoss) {//YearOnYearLoss

                changed = true;
                record.stored.AnnualAdjustment = event.value_new;
            } else if (isConversion) {//Conversion

                changed = true;
                record.stored.NtiAdjustment = event.value_new;
            }
            //this marks not dirty
            //if (changed) this.$RatingsBooksGrid.set(event.recid, { hasChange: true });
            record.hasChange = changed;
            //console.log('onRatingsGridChange', record);
        },

        //save books based on stored data in each record
        onBooksSave: function () {
            var ret = [];
            $.each(this.$RatingsBooksGrid.records, function (index, book) {
                if (!book.addNew) ret.push(book.stored);
            });
            //console.log('onBooksSave', ret);
            _view.controller.apiSaveRatingsBooks(ret, this.afterBooksSave.bind(this));

        },
        //folowing save close and reset
        afterBooksSave: function (response) {
            this.$Modal.modal('hide');
            util.notify('Ratings Book Saved');
            this.activeRatingsBooks = [];
            this.postingBooks = [];
        }

    }

};