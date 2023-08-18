using Markdig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tMDViewer.Prism
{
    internal static class MarkdigExtensions
    {
        public static MarkdownPipelineBuilder UsePrism(this MarkdownPipelineBuilder pipeline)
        {
            pipeline.Extensions.Add(new PrismExtension());
            return pipeline;
        }
    }
}
