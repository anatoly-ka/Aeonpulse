using System;

namespace Aeonpulse.Attributes
{
    /// <summary>
    /// Marks a class or method as a critical business-logic node for AI Knowledge Graph analysis.
    /// The <paramref name="role"/> describes the semantic role this symbol plays in the system
    /// (e.g., "CoreCalculation", "StateOrchestrator", "PersistenceGateway").
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property,
                    AllowMultiple = true,
                    Inherited = false)]
    public sealed class AIContextAttribute : Attribute
    {
        /// <summary>
        /// Gets the semantic role label assigned to the decorated symbol.
        /// </summary>
        public string Role { get; }

        /// <param name="role">
        /// A short, noun-phrase label such as "CoreCalculation" or "StateOrchestrator"
        /// that classifies the symbol's purpose in the AI Knowledge Graph.
        /// </param>
        public AIContextAttribute(string role) => Role = role;
    }
}