namespace Microsoft.AspNetCore.Diagnostics
{
    public partial class CompilationFailure
    {
        public CompilationFailure(string sourceFilePath, string sourceFileContent, string compiledContent, System.Collections.Generic.IEnumerable<Microsoft.AspNetCore.Diagnostics.DiagnosticMessage> messages) { }
        public string CompiledContent { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.Collections.Generic.IEnumerable<Microsoft.AspNetCore.Diagnostics.DiagnosticMessage> Messages { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public string SourceFileContent { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public string SourceFilePath { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
    }
    public partial class DiagnosticMessage
    {
        public DiagnosticMessage(string message, string formattedMessage, string filePath, int startLine, int startColumn, int endLine, int endColumn) { }
        public int EndColumn { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public int EndLine { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public string FormattedMessage { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public string Message { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public string SourceFilePath { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public int StartColumn { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public int StartLine { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
    }
    public partial interface ICompilationException
    {
        System.Collections.Generic.IEnumerable<Microsoft.AspNetCore.Diagnostics.CompilationFailure> CompilationFailures { get; }
    }
    public partial interface IExceptionHandlerFeature
    {
        System.Exception Error { get; }
    }
    public partial interface IStatusCodePagesFeature
    {
        bool Enabled { get; set; }
    }
    public partial interface IStatusCodeReExecuteFeature
    {
        string OriginalPath { get; set; }
        string OriginalPathBase { get; set; }
    }
}
