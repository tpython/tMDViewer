using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Markdig;
using tMDViewer.Prism;

namespace tMDViewer
{
    internal class Engine
    {
        public Engine()
        {
            pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UsePrism().Build();
        }

        MarkdownPipeline pipeline;

        public string ToHtml(string content)
        {
            var mdhtml = Markdown.ToHtml(content, pipeline);

            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            var template = File.ReadAllText(Path.Combine(dir, @"resources/Template.html"));
            return template.Replace("#BODY#", mdhtml);
        }

        public string ToText(string content)
        {
            return Markdown.ToPlainText(content, pipeline);
        }
    }
}
