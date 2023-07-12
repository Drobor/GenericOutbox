using System.Text;

namespace GenericOutbox.SourceGenerator
{
    public class CodeBuilder
    {
        private readonly StringBuilder _sb = new StringBuilder();
        private int offset = 0;

        public void WriteCode(params string[] lines)
        {
            foreach (var line in lines)
            {
                if (line.StartsWith("}"))
                {
                    offset -= 4;
                }

                _sb.AppendLine(new string(' ', offset) + line);

                if (line.StartsWith("{"))
                {
                    offset += 4;
                }
            }
        }

        public override string ToString() => _sb.ToString();
    }
}
