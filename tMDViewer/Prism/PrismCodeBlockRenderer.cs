using Markdig.Parsers;
using Markdig.Renderers.Html;
using Markdig.Renderers;
using Markdig.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace tMDViewer.Prism
{
    internal class PrismCodeBlockRenderer : HtmlObjectRenderer<CodeBlock>
    {
        private readonly CodeBlockRenderer codeBlockRenderer;

        public PrismCodeBlockRenderer(CodeBlockRenderer codeBlockRenderer)
        {
            this.codeBlockRenderer = codeBlockRenderer ?? new CodeBlockRenderer();
        }

        protected override void Write(HtmlRenderer renderer, CodeBlock node)
        {
            var fencedCodeBlock = node as FencedCodeBlock;
            var parser = node.Parser as FencedCodeBlockParser;
            if (fencedCodeBlock == null || parser == null)
            {
                codeBlockRenderer.Write(renderer, node);
                return;
            }

            var languageCode = fencedCodeBlock.Info.Replace(parser.InfoPrefix, string.Empty)?.ToLower();
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                codeBlockRenderer.Write(renderer, node);
                return;
            }

            if (languageCode == "c#")
            {
                languageCode = "cs";
            }

            var attributes = new HtmlAttributes();
            attributes.AddClass($"language-{languageCode}");

            var code = ExtractSourceCode(node);
            var escapedCode = HttpUtility.HtmlEncode(code);

            renderer
                .Write("<pre>")
                .Write("<code")
                .WriteAttributes(attributes)
                .Write(">")
                .Write(escapedCode)
                .Write("</code>")
                .Write("</pre>");
        }

        protected string ExtractSourceCode(LeafBlock node)
        {
            var code = new StringBuilder();
            var lines = node.Lines.Lines;
            int totalLines = lines.Length;
            for (int i = 0; i < totalLines; i++)
            {
                var line = lines[i];
                var slice = line.Slice;
                if (slice.Text != null)
                {
                    var lineText = slice.Text.Substring(slice.Start, slice.Length);
                    if (i > 0)
                    {
                        code.AppendLine();
                    }

                    code.Append(lineText);
                }
            }

            return code.ToString();
        }
    }
}
