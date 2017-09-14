//Common Utility methods; 
//TODO: possibly enclosure module pattern; remove obsolete/project specific methods; remove prototypes
Array.prototype.remove = function (predicate) {
    var idxs = [];
    for (var i = 0, j = this.length; i < j; i++) {
        if (predicate(this[i], i, this)) {
            idxs.push(i);
        }
    }

    for (var i = 0; i < idxs.length; i++) {
        this.splice(idxs[i] - i, 1);
    }
}
Object.defineProperty(Array.prototype, "remove", { enumerable: false });