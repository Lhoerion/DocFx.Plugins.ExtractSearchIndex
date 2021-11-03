(function () {
  importScripts('lunr.min.js');

  var lunrIndex;

  var searchData = {};

  var searchDataRequest = new XMLHttpRequest();
  var searchIndexRequest = new XMLHttpRequest();

  searchDataRequest.open('GET', '../index.json');
  searchDataRequest.onload = function () {
    if (this.status !== 200) {
      return;
    }
    searchData = JSON.parse(this.responseText);

    searchIndexRequest.send();
  }
  searchIndexRequest.open('GET', '../search-index.json');
  searchIndexRequest.onload = function () {
    if (this.status !== 200) {
      return;
    }
    lunrIndex = lunr.Index.load(JSON.parse(resp));

    postMessage({ e: 'index-ready' });
  }
  searchIndexRequest.send();

  onmessage = function (oEvent) {
    var q = oEvent.data.q;
    var hits = lunrIndex.search(q);
    var results = [];
    hits.forEach(function (hit) {
      var item = searchData[hit.ref];
      results.push({ 'type': item.type, 'href': item.href, 'title': item.title, 'keywords': item.keywords, 'langs': item.langs });
    });
    postMessage({ e: 'query-ready', q: q, d: results });
  }
})();
