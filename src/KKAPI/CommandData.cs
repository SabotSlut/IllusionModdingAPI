using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ADV;
using BepInEx;

namespace KKAPI
{
    /// <summary>
    /// Contains data about a custom command.
    /// </summary>
    public class CustomCommand
    {
        /// <summary>
        /// The internal ID of the custom command.
        /// </summary>
        public Command ID;
        /// <summary>
        /// The type of the custom command.
        /// </summary>
        public Type Type;
        /// <summary>
        /// The plugin that owns the custom command.
        /// </summary>
        public BaseUnityPlugin Owner;

        /// <summary>
        /// Creates a new custom command with the supplied information.
        /// </summary>
        /// <param name="id">The internal ID of the custom command.</param>
        /// <param name="type">The type of the custom command.</param>
        /// <param name="owner">The plugin that owns the custom command.</param>
        public CustomCommand(Command id, Type type, BaseUnityPlugin owner)
        {
            ID = id;
            Type = type;
            Owner = owner;
        }
    }
}
