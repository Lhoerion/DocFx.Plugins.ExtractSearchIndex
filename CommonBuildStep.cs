namespace DocFx.Plugins.ExtractSearchIndex
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;

    using Microsoft.DocAsCode.Plugins;

    [Export("ManagedReferenceDocumentProcessor", typeof(IDocumentBuildStep))]
    [Export("UniversalReferenceDocumentProcessor", typeof(IDocumentBuildStep))]
    [Export("TypeScriptReferenceDocumentProcessor", typeof(IDocumentBuildStep))]
    public class CommonBuildStep : IDocumentBuildStep
    {
        public int BuildOrder => 0;

        public string Name => nameof(CommonBuildStep);

        public IEnumerable<FileModel> Prebuild(ImmutableList<FileModel> models, IHostService host) { return models; }

        public void Build(FileModel model, IHostService host) { }

        public void Postbuild(ImmutableList<FileModel> models, IHostService host)
        {
            foreach (var model in models)
            {
                var list = new List<string>();
                foreach (var item in ((dynamic) model.Content).Items)
                {
                    foreach (string el in item.SupportedLanguages)
                    {
                        if (list.Contains(el)) continue;
                        list.Add(el);
                    }
                }
                model.ManifestProperties.langs = list;
            }
        }
    }
}