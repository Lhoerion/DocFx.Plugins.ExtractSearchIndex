using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Composition;
using System.Collections.Immutable;
using DocFx.Plugins.ExtractSearchIndex.Lunr;
using Microsoft.DocAsCode.Common;
using Microsoft.DocAsCode.Plugins;

using HtmlAgilityPack;

namespace DocFx.Plugins.ExtractSearchIndex
{
    [Export(nameof(ExtractSearchIndex) + "Alt", typeof(IPostProcessor))]
    // ReSharper disable once UnusedType.Global
    public class ExtractSearchIndex : IPostProcessor
    {
        private string _lunrTokenSeparator;

        private string _lunrRef;

        private Dictionary<string, object> _lunrFields;

        private List<string> _lunrStopWords;

        private List<string> _lunrMetadataWhitelist;

        public ImmutableDictionary<string, object> PrepareMetadata(ImmutableDictionary<string, object> metadata)
        {
            if (!metadata.ContainsKey("_enableSearch"))
            {
                metadata = metadata.Add("_enableSearch", true);
            }

            if (metadata.TryGetValue("_lunrTokenSeparator", out var lunrTokenSeparator))
            {
                _lunrTokenSeparator = (string)lunrTokenSeparator;
            }

            if (metadata.TryGetValue("_lunrRef", out var lunrRef))
            {
                _lunrRef = (string)lunrRef;
            }

            if (metadata.TryGetValue("_lunrFields", out var lunrFields))
            {
                _lunrFields = (Dictionary<string, object>)lunrFields;
            }

            if (metadata.TryGetValue("_lunrStopWords", out var lunrStopWords))
            {
                _lunrStopWords = (List<string>)lunrStopWords;
            }

            if (metadata.TryGetValue("_lunrStopWords", out var lunrMetadataWhitelist))
            {
                _lunrMetadataWhitelist = (List<string>)lunrMetadataWhitelist;
            }

            return metadata;
        }

        public Manifest Process(Manifest manifest, string outputFolder)
        {
            if (outputFolder == null)
            {
                throw new ArgumentException("Base directory can not be null");
            }

            var indexData = new SortedDictionary<string, SearchIndexItem>();
            var indexDataPath = Path.Combine(outputFolder, "index.json");
            var htmlFiles = (from item in manifest.Files ?? Enumerable.Empty<ManifestItem>()
                from output in item.OutputFiles
                where item.DocumentType != "Toc" && output.Key.Equals(".html", StringComparison.OrdinalIgnoreCase)
                select (output.Value.RelativePath, item)).ToList();
            if (htmlFiles.Count == 0)
            {
                return manifest;
            }

            Logger.LogInfo($"Extracting index data from {htmlFiles.Count} html files");
            foreach (var (relativePath, item) in htmlFiles)
            {
                var filePath = Path.Combine(outputFolder, relativePath);
                var html = new HtmlDocument();
                Logger.LogDiagnostic($"Extracting index data from {filePath}");

                if (!EnvironmentContext.FileAbstractLayer.Exists(filePath)) continue;
                try
                {
                    using var stream = EnvironmentContext.FileAbstractLayer.OpenRead(filePath);
                    html.Load(stream, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning($"Warning: Can't load content from {filePath}: {ex.Message}");
                    continue;
                }
                var indexItem = SearchIndex.ExtractItem(html, relativePath, item);
                if (indexItem != null)
                {
                    indexData[relativePath] = indexItem;
                }
            }
            JsonUtility.Serialize(indexDataPath, indexData);

            var manifestItem = new ManifestItem
            {
                DocumentType = "Resource",
            };
            manifestItem.OutputFiles.Add("resource", new OutputFileInfo
            {
                RelativePath = PathUtility.MakeRelativePath(outputFolder, indexDataPath),
            });
            manifest.Files?.Add(manifestItem);

            var lunrIndex = Lunr.Lunr.Main(builder =>
            {
                try
                {
                    var _ = Regex.IsMatch("__dummy__", _lunrTokenSeparator);
                    Tokenizer.TokenSeparator = _lunrTokenSeparator;
                }
                catch(ArgumentException)
                {
                    Logger.LogWarning("[Lunr]Warning: Invalid token separator provided, fallback to default");
                }

                if (_lunrStopWords != null)
                {
                    StopWordFilter.CustomStopWords.AddRange(_lunrStopWords);
                }
                else
                {
                    Logger.LogDiagnostic("[Lunr]No custom search stop words provided, skipping...");
                }

                if (_lunrMetadataWhitelist != null)
                {
                    builder.MetadataWhitelist.AddRange(_lunrMetadataWhitelist);
                }
                else
                {
                    Logger.LogDiagnostic("[Lunr]No metadata whitelist provided, skipping...");
                }

                if (!string.IsNullOrEmpty(_lunrRef))
                {
                    builder.Ref(_lunrRef);
                }

                if (_lunrFields != null)
                {
                    foreach (var field in _lunrFields)
                    {
                        builder.Field(field.Key, field.Value.ToJsonString().FromJsonString<FieldRef.FieldMetadata>());
                    }
                }
                else
                {
                    Logger.LogWarning("[Lunr]No fields provided, this may yield strange results");
                }

                foreach (var doc in indexData)
                {
                    builder.Add(doc.Value, new FieldRef.FieldMetadata());
                }
            });
            var searchIndexPath = Path.Combine(outputFolder, "search-index.json");
            JsonUtility.Serialize(searchIndexPath, lunrIndex.ToJson());

            manifestItem = new ManifestItem
            {
                DocumentType = "Resource",
            };
            manifestItem.OutputFiles.Add("resource", new OutputFileInfo
            {
                RelativePath = PathUtility.MakeRelativePath(outputFolder, searchIndexPath),
            });
            manifest.Files?.Add(manifestItem);

            return manifest;
        }
    }
}
