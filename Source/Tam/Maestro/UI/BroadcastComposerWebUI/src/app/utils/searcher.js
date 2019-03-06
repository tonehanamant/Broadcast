import FuzzySearch from "fuzzy-search";

const searcher = (data, searchKeys, query) => {
  const searcher = new FuzzySearch(data, searchKeys, {
    caseSensitive: false
  });
  return searcher.search(query);
};

export default searcher;
