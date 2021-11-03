namespace DocFx.Plugins.ExtractSearchIndex
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;

    using Microsoft.DocAsCode.Plugins;

    [Export("ConceptualDocumentProcessor", typeof(IDocumentBuildStep))]
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
                if (model.Content is Dictionary<string, object> content)
                {
                    if (!content.TryGetValue("langs", out var langs)) continue;
                    model.ManifestProperties.langs = new List<string>(((object[])langs).Cast<string>());
                }
                else
                {
                    if (!((dynamic)model.Content).Metadata.TryGetValue("langs", out object langs)) continue;
                    model.ManifestProperties.langs = new List<string>(((object[])langs).Cast<string>());
                }
            }
        }
    }
}