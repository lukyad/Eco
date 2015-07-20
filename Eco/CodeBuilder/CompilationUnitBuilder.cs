using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco
{
    class CompilationUnitBuilder : FragmentBuilder
    {
		readonly FragmentBuilder _assemblyAttributesBuilder = new FragmentBuilder();
		readonly BlockBuilder _compilationUnit = new BlockBuilder();

		public CompilationUnitBuilder(string unitNamespace)
        {
			base.AddPart(_assemblyAttributesBuilder);
			base.AddPart("namespace " + unitNamespace);
			base.AddPart(_compilationUnit);
        }

		public void AddAssemblyAttribute(string attribute)
		{
			_assemblyAttributesBuilder.AddPart("[assembly: " + attribute + "]");
		}

		public ClassBuilder AddClass(string className, string derivedFrom = null)
        {
            return _compilationUnit.AddPartBuilder(new ClassBuilder(className, derivedFrom));
        }
	}
}
