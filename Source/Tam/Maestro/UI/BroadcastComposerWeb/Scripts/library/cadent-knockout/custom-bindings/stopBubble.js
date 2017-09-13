/*** prevents the binded element click event from bubbling (http://stackoverflow.com/questions/14321012/prevent-event-bubbling-when-using-the-checked-binding-in-knockoutjs) ***/

ko.bindingHandlers.stopBubble = {
    init: function (element) {
        ko.utils.registerEventHandler(element, "click", function (event) {
            event.cancelBubble = true;
            if (event.stopPropagation) {
                event.stopPropagation();
            }
        });
    }
};