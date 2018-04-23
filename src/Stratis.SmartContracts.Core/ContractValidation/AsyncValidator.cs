using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace Stratis.SmartContracts.Core.ContractValidation
{
    /// <summary>
    /// Validates that a <see cref="Mono.Cecil.TypeDefinition"/> does not contain any async methods
    /// </summary>
    public class AsyncValidator : ITypeDefinitionValidator
    {
        public static string AsyncStateMachine = typeof(System.Runtime.CompilerServices.IAsyncStateMachine).FullName;

        public IEnumerable<SmartContractValidationError> Validate(TypeDefinition module)
        {
            // Async methods each have a compiler-generated nested type implementation of System.Runtime.CompilerServices.IAsyncStateMachine
            if (module.HasNestedTypes)
            {
                Collection<TypeDefinition> nestedTypes = module.NestedTypes;

                if (nestedTypes.Any(IsAsyncStateMachine))
                {
                    return new List<SmartContractValidationError>
                    {
                        new SmartContractValidationError("Async methods are not allowed")
                    };
                }
            }

            return Enumerable.Empty<SmartContractValidationError>();
        }

        private static bool IsAsyncStateMachine(TypeDefinition type)
        {
            return type.Interfaces.Any(i =>
                i.InterfaceType.FullName.Equals(AsyncStateMachine));
        }
    }
}