(function () {
  importScripts('https://cdnjs.cloudflare.com/ajax/libs/lunr.js/2.3.9/lunr.min.js');

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
    lunrIndex = lunr.Index.load(JSON.parse(this.responseText));

    postMessage({ e: 'index-ready' });
  }

  searchDataRequest.send();

  onmessage = function (oEvent) {
    var q = oEvent.data.q;
    var hits = lunrIndex.search(q.split(/\s+/g).map(term => {
      return !term.startsWith('-') ? (!term.startsWith('+') ? '+' + term : term.substring(1)) : term;
    }).join(' '));
    var results = [];
    hits.sort((a, b) => (searchData[a.ref].type > searchData[b.ref].type) - (searchData[a.ref].type < searchData[b.ref].type));
    hits.forEach(function(hit) {
      results.push(searchData[hit.ref]);
    });
    postMessage({ e: 'query-ready', q: q, d: results });
  }
})();
