function stickyTitles(scrollArea, stickies, holdWidth, offset) {

    this.load = function() {
        if (typeof holdWidth == "undefined") holdWidth = false;
        if (typeof offset == "undefined") offset = 0;
        
        scrollArea.on("scroll.sticky", this.scroll);

        stickies.each( function(i){
            var $t = $(this);
            $t.data("origpos", $t.position().top);
        });
    };
        
    this.scroll = function() {
        var visibleStickies = stickies.filter(":visible");

        visibleStickies.each(function(i){
            var thisSticky = $(this),   // current sticky
                nextSticky = visibleStickies.eq(i+1); // next sticky
                scrollTop = scrollArea.scrollTop(), // current scroll position
                pos = thisSticky.position().top,    // sticky top position
                orgPos = thisSticky.data("origpos");    // original sticky top position

            if (offset > 0) scrollTop += offset;
            if (holdWidth) sizeColumns(thisSticky);

            // sticky position is greater than the current scroll position
            if (pos > scrollTop) {
                if (scrollTop === offset || orgPos >= pos || orgPos >= scrollTop) {
                    // check if at very top, the org position is greather than sticky position, or org position is greater than current scroll position
                    thisSticky.removeClass("keepInView");
                    thisSticky.css("top", "initial");
                } else {
                    // otherwise make sticky
                    thisSticky.css('top',  scrollTop);
                    thisSticky.addClass("keepInView");
                }    
            } else {
                // sticky position is lower then current scroll position
                var boundry = i + 1 < visibleStickies.length ? nextSticky.data("origpos") - thisSticky.outerHeight() : scrollArea[0].scrollHeight - thisSticky.outerHeight();

                if (scrollTop < boundry) {
                    thisSticky.css('top',  scrollTop);
                } else {
                    thisSticky.css('top',  boundry);
                }

                thisSticky.addClass("keepInView");
            } 
        }); 
    };

    sizeColumns = function(s) {
        var $tds = $(s).find("td");
        var styles = "";
        
        $tds.each(function(i){
            var width = $(this).outerWidth();
            styles += ".stickyColumn" + i + " { width: " + width + "px; } ";
            $(this).addClass("stickyColumn" + i);
        });

        $("#stickyColumns").remove();
        $("<style id='stickyColumns'>" + styles + "</style>").appendTo(document.head);
    };
}