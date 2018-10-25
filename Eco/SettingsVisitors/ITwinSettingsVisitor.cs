using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Eco
{
    /// <summary>
    /// Refined settings visitors get invoked by the Eco library in the following cases:
    /// 
    /// 1. On settings load, right after the raw settings visitors.
    /// When raw settings visitors complete thier operations the raw settins graph is fully initialized.
    /// Refined settings visitors initialize the parallel refined settings graph.
    /// 
    /// 2. On settings save, as the very first operation on the refined settings graph.
    /// The purpose of the refined settins visitors here is to initialize the raw settings graph
    /// and prepare it for the serialization.
    /// </summary>
    public interface ITwinSettingsVisitor
    {
        /// <summary>
        /// A visitor is considered to be reversable if the changes it makes can be 
        /// undone by another visitor.
        /// 
        /// Some visitors (eg EnvironmentVariableExpander) can make non-reversable changes.
        /// If 'non-reversable' visitors are used during Load operation, then settings can not be saved
        /// back in the same format.
        /// 
        /// All 'non-reversable' visitors can be skipped by setting the 'skipNonReversableOperations' flag when loading settings.
        /// </summary>
        bool IsReversable { get; }

        /// <summary>
        /// Gets called by the Eco library ones on each Load/Save operation.
        /// Allows to initialize visitor before the fields processing start.
        /// </summary>
        void Initialize(Type rootMasterSettingsType, Type rootSlaveSettingsType);

        /// <summary>
        /// Gets called ones for all objects of a settings type in two twin settings trees.
        /// </summary>
        void Visit(string settingsNamespace, string settingsPath, object masterSettings, object slaveSettings);

        /// <summary>
        /// Gets called ones for each field for all objects of a settings type in the two twin setting trees.
        /// </summary>
        void Visit(string settingsNamespace, string fieldPath, object masterSettings, FieldInfo masterSettingsField, object slaveSettings, FieldInfo slaveSettingsField);
    }
}
