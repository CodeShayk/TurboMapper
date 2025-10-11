using System.Collections.Generic;

namespace TurboMapper
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public IEnumerable<string> Errors { get; set; }
        
        public ValidationResult()
        {
            Errors = new List<string>();
        }
        
        public ValidationResult(bool isValid, IEnumerable<string> errors)
        {
            IsValid = isValid;
            Errors = errors ?? new List<string>();
        }
    }
}