/* eslint-disable */
export class Sorter {
  sortBy(name, direction, datasource) {
    return datasource.data.sort((a, b) => {
      if (a.get(name) < b.get(name) && direction) {
        return direction === "ASC" ? 1 : -1;
      } else if (a.get(name) > b.get(name)) {
        return direction === "ASC" ? -1 : 1;
      }
    });
  }
}

export default new Sorter();
