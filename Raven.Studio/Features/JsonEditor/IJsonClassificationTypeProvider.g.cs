namespace Raven.Studio.Features.JsonEditor {
    using ActiproSoftware.Text;
    
    
    /// <summary>
    /// Provides the base requirements of an object that provides <see cref="IClassificationType"/> objects for the <c>Json</c> language.
    /// </summary>
    /// <remarks>
    /// This type was generated by the Actipro Language Designer tool v12.1.561.0 (http://www.actiprosoftware.com).
    /// </remarks>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("LanguageDesigner", "12.1.561.0")]
    public interface IJsonClassificationTypeProvider {
        
        /// <summary>
        /// Gets the <c>Delimiter</c> classification type.
        /// </summary>
        /// <value>The <c>Delimiter</c> classification type.</value>
        IClassificationType Delimiter {
            get;
        }
        
        /// <summary>
        /// Gets the <c>Keyword</c> classification type.
        /// </summary>
        /// <value>The <c>Keyword</c> classification type.</value>
        IClassificationType Keyword {
            get;
        }
        
        /// <summary>
        /// Gets the <c>Number</c> classification type.
        /// </summary>
        /// <value>The <c>Number</c> classification type.</value>
        IClassificationType Number {
            get;
        }
        
        /// <summary>
        /// Gets the <c>Operator</c> classification type.
        /// </summary>
        /// <value>The <c>Operator</c> classification type.</value>
        IClassificationType Operator {
            get;
        }
        
        /// <summary>
        /// Gets the <c>String</c> classification type.
        /// </summary>
        /// <value>The <c>String</c> classification type.</value>
        IClassificationType String {
            get;
        }
    }
}