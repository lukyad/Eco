using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Eco
{
    /// <summary>
    /// Raw settins visitors get invoked by the Eco library in the following cases:
    /// 
    /// 1. On settins load, right after raw settings object graph has been deserialized from a stream.
    /// The purpose of the raw settings vistitors in this case is to add extra processing of the raw data (if needed),
    /// before it passed to the refined settings visitors.
    /// 
    /// 2. On settins save, right after the refined settings visitors have completed thier operations.
    /// By this stage the raw settings graph is already initialized and ready for the serialization.
    /// The raw settings visitors can make some additional data transformations (if required).
    /// </summary>
    public interface ISettingsVisitor
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
        void Initialize(Type rootSettingsType);

        /// <summary>
        /// Gets called ones for all objects of a settings type in the settings tree.
        /// </summary>
        void Visit(string settingsNamespace, string settingsPath, object settings);

        /// <summary>
        /// Gets called ones for each field for all objects of a settings type in the settings tree.
        /// </summary>
        void Visit(string settingsNamespace, string fieldPath, object settings, FieldInfo settingsField);
    }
}
