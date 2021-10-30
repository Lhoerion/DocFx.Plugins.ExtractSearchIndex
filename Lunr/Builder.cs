#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DocFx.Plugins.ExtractSearchIndex.Lunr
{
    public class Builder
    {
        private string _ref = "id";

        private Dictionary<string, FieldRef.FieldMetadata?> _fields = new Dictionary<string, FieldRef.FieldMetadata?>();

        private Dictionary<string, FieldRef.FieldMetadata> _documents = new Dictionary<string, FieldRef.FieldMetadata>();

        public readonly Pipeline Pipeline = new Pipeline();

        public readonly Pipeline SearchPipeline = new Pipeline();

        public Dictionary<FieldRef, Dictionary<Token, int>> FieldTermFrequencies = new Dictionary<FieldRef, Dictionary<Token, int>>();

        public Dictionary<FieldRef, int> FieldLengths = new Dictionary<FieldRef, int>();

        public Dictionary<string, double> AverageFieldLength = new Dictionary<string, double>();

        public Dictionary<Token, Dictionary<string, object>> InvertedIndex = new Dictionary<Token, Dictionary<string, object>>(new Token.EqualityComparer());

        public Dictionary<FieldRef, Vector> FieldVectors = new Dictionary<FieldRef, Vector>(new FieldRef.EqualityComparer());

        public int DocumentCount;

        public int TermIndex;

        // ReSharper disable once CollectionNeverUpdated.Global
        public readonly List<string> MetadataWhitelist = new List<string>();

        private double _b = 0.75;

        private double _k1 = 1.2;

        public void Ref(string rRef) {
            _ref = rRef;
        }

        public void Field(string fieldName, FieldRef.FieldMetadata? attributes)
        {
            if (new Regex("\\/").IsMatch(fieldName))
            {
                throw new Exception("Field '" + fieldName + "' contains illegal character '\\'.");
            }

            _fields[fieldName] = attributes;
        }

        public void Add(Dictionary<string, string> doc, FieldRef.FieldMetadata attributes)
        {
            var docRef = doc[_ref];
            var fields = new List<string>(_fields.Keys);
            _documents[docRef] = attributes;
            DocumentCount += 1;

            for (var i = 0; i < fields.Count; i++)
            {
                var fieldName = fields[i];
                var extractor = _fields[fieldName]?.Extractor;
                var field = extractor != null ? extractor(doc) : doc[fieldName];
                var tokens = Utils.Tokenizer(field, new Dictionary<string, object>
                {
                    {
                        "fields", new List<string>
                        {
                            fieldName
                        }
                    }
                });

                var terms = Pipeline.Run(tokens);
                var fieldRef = new FieldRef(docRef, fieldName);
                var fieldTerms = FieldTermFrequencies.ContainsKey(fieldRef) ? FieldTermFrequencies[fieldRef] : new Dictionary<Token, int>(new Token.EqualityComparer());

                FieldTermFrequencies[fieldRef] = fieldTerms;
                FieldLengths[fieldRef] = 0;
                FieldLengths[fieldRef] += terms.Count;

                for (var j = 0; j < terms.Count; j++)
                {
                    var term = terms[j];

                    if (!fieldTerms.ContainsKey(term))
                    {
                        fieldTerms[term] = 0;
                    }

                    fieldTerms[term] += 1;

                    if (!InvertedIndex.ContainsKey(term))
                    {
                        var posting = new Dictionary<string, object>();
                        posting["_index"] = TermIndex;
                        TermIndex += 1;

                        for (var k = 0; k < fields.Count; k++)
                        {
                            posting[fields[k]] = new Dictionary<string, Dictionary<string, object>>();
                        }

                        InvertedIndex[term] = posting;
                    }

                    if (!((Dictionary<string, dynamic>)InvertedIndex[term])[fieldName].ContainsKey(docRef))
                    {
                        ((Dictionary<string, dynamic>)InvertedIndex[term])[fieldName][docRef] =
                            new Dictionary<string, object>();
                    }

                    for (var l = 0; l < MetadataWhitelist.Count; l++)
                    {
                        var metadataKey = MetadataWhitelist[l];
                        var metadata = term.Metadata[metadataKey];

                        if (!((Dictionary<string, dynamic>)InvertedIndex[term])[fieldName][docRef].ContainsKey(metadataKey)) {
                            ((Dictionary<string, dynamic>)InvertedIndex[term])[fieldName][docRef][metadataKey] =
                                new List<object>();
                        }

                        ((Dictionary<string, dynamic>)InvertedIndex[term])[fieldName][docRef][metadataKey].Add(metadata);
                    }
                }
            }
        }

        public Index Build()
        {
            CalculateAverageFieldLengths();
            CreateFieldVectors();
            return new Index(InvertedIndex, FieldVectors, new List<string>(_fields.Keys), SearchPipeline);
        }

        private void CalculateAverageFieldLengths()
        {
            var fieldRefs = new List<FieldRef>(FieldLengths.Keys);
            var numberOfFields = fieldRefs.Count;
            var accumulator = new Dictionary<string, double>();
            var documentsWithField = new Dictionary<string, double>();

            for (var i = 0; i < numberOfFields; i++)
            {
                var fieldRef = FieldRef.FromString(fieldRefs[i].ToString());
                var field = fieldRef.FieldName; // ?

                if (!documentsWithField.ContainsKey(field))
                {
                    documentsWithField[field] = 0;
                }
                documentsWithField[field] += 1;

                if (!accumulator.ContainsKey(field))
                {
                    accumulator[field] = 0;
                }
                accumulator[field] += FieldLengths.First(pair => pair.Key.ToString() == fieldRef.ToString()).Value;
            }

            var fields = new List<string>(_fields.Keys);

            for (var i = 0; i < fields.Count; i++)
            {
                var fieldName = fields[i];
                accumulator[fieldName] /= documentsWithField[fieldName];
            }

            AverageFieldLength = accumulator;
        }

        private void CreateFieldVectors()
        {
            var fieldVectors = new Dictionary<FieldRef, Vector>();
            var fieldRefs = new List<FieldRef>(FieldTermFrequencies.Keys);
            var fieldRefsLength = fieldRefs.Count;
            var termIdfCache = new Dictionary<Token, double>();

            for (var i = 0; i < fieldRefsLength; i++)
            {
                var fieldRef = FieldRef.FromString(fieldRefs[i].ToString());
                var fieldName = fieldRef.FieldName;
                var fieldLength = FieldLengths.First(pair => pair.Key.ToString() == fieldRef.ToString()).Value;
                var fieldVector = new Vector();
                var termFrequencies = FieldTermFrequencies.First(pair => pair.Key.ToString() == fieldRef.ToString()).Value;
                var terms = new List<Token>(termFrequencies.Keys);
                var termsLength = terms.Count;

                var fieldBoost = _fields[fieldName]?.Boost ?? 1;
                var docBoost = _documents[fieldRef.DocRef]?.Boost ?? 1;

                for (var j = 0; j < termsLength; j++)
                {
                    var term = terms[j];
                    var tf = termFrequencies[term];
                    var termIndex = (int)InvertedIndex[term]["_index"];
                    double idf, score, scoreWithPrecision;

                    if (!termIdfCache.ContainsKey(term))
                    {
                        idf = Utils.Idf(InvertedIndex[term], DocumentCount);
                        termIdfCache[term] = idf;
                    }
                    else
                    {
                        idf = termIdfCache[term];
                    }

                    score = idf * ((_k1 + 1) * tf) /
                            (_k1 * (1 - _b + _b * (fieldLength / AverageFieldLength[fieldName])) + tf);
                    score *= fieldBoost;
                    score *= docBoost;
                    scoreWithPrecision = Math.Round(score * 1000) / 1000;
                    // Converts 1.23456789 to 1.234.
                    // Reducing the precision so that the vectors take up less
                    // space when serialised. Doing it now so that they behave
                    // the same before and after serialisation. Also, this is
                    // the fastest approach to reducing a number's precision in
                    // JavaScript.

                    fieldVector.Insert(termIndex, scoreWithPrecision);
                }

                fieldVectors[fieldRef] = fieldVector;
            }

            FieldVectors = fieldVectors;
        }
    }
}