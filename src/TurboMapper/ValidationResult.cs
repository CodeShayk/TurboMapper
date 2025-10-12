using System.Collections.Generic;

namespace TurboMapper
{
    /// <summary>
    /// Represents the result of a validation operation, including whether it was successful and any associated error messages.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Indicates whether the validation was successful.
        /// </summary>
        public bool IsValid { get; set; }
        /// <summary>
        /// A collection of error messages associated with the validation.
        /// </summary>
        public IEnumerable<string> Errors { get; set; }
        /// <summary>
        /// Initializes a new instance of the ValidationResult class with default values.
        /// </summary>
        public ValidationResult()
        {
            Errors = new List<string>();
        }
        /// <summary>
        /// Initializes a new instance of the ValidationResult class with specified validity and error messages.
        /// </summary>
        /// <param name="isValid"></param>
        /// <param name="errors"></param>
        public ValidationResult(bool isValid, IEnumerable<string> errors)
        {
            IsValid = isValid;
            Errors = errors ?? new List<string>();
        }
    }
}