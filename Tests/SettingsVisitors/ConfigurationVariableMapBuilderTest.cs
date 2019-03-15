using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using NHamcrest;
using NHamcrest.Core;
using Eco;
using Eco.SettingsVisitors;

namespace Tests.SettingsVisitors
{
    public class ConfigurationVariableMapBuilderTest : SettingsVisitorTestBase
    {
        class settings
        {
            public variable var;
            public variable[] varArray;
        }

        [Fact]
        public static void BuildValidVarMap()
        {
            var settings = new settings
            {
                var = new variable { name = "var1", value = "value1" },
                varArray = new[]
                {
                    new variable { name="Var2", value="value2"},
                    new variable { name="var_3", value="value3"},
                    new variable { name="444", value="value4"},
                    new variable { name="_", value="value5"}
                }
            };
            var fieldVisitor = new ConfigurationVariableMapBuilder();
            SettingsManager.TraverseSeetingsTree(
                startNamespace: null,
                startPath: null,
                rootMasterSettings: settings,
                visitor: fieldVisitor);
            Assert.That(fieldVisitor.Variables.Keys, Has.Items(
                Is.EqualTo("var1"),
                Is.EqualTo("Var2"),
                Is.EqualTo("var_3"),
                Is.EqualTo("444"),
                Is.EqualTo("_")));
            //Assert.That(fieldVisitor.Variables.Values, Has.Items("value1", "value2", "value3", "value4", "value5"));
        }

        [Fact]
        public static void ThrowsDuplicatedVar()
        {
            var settings = new settings
            {
                var = new variable { name = "var1", value = "value1" },
                varArray = new[]{ new variable { name="var1", value="value2"} }
            };
            var fieldVisitor = new ConfigurationVariableMapBuilder();
            Assert.That(
                () => SettingsManager.TraverseSeetingsTree(startNamespace: null, startPath: null, rootMasterSettings: settings, visitor: fieldVisitor), 
                Throws.An<ConfigurationException>());
        }

        [Fact]
        public static void ThrowsInvalidVarName()
        {
            string[] invalidVarNames = new string[] { null, "" };
            var fieldVisitor = new ConfigurationVariableMapBuilder();
            foreach (var varName in invalidVarNames)
            {
                var settings = new settings { var = new variable { name = varName } };
                Assert.That(
                    () => SettingsManager.TraverseSeetingsTree(startNamespace: null, startPath: null, rootMasterSettings: settings, visitor: fieldVisitor),
                    Throws.An<ConfigurationException>());
            }
        }

    }
}
