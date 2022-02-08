﻿using Synapse.Api.Plugin;
using System;
using System.Collections.Generic;
using VT_Api.Core.Plugin.AutoRegisterProcess;
using VT_Api.Reflexion;

namespace VT_Api.Core.Plugin
{
    public class AutoRegisterManager
    {
        internal AutoRegisterManager() { }


        readonly IContextProcessor[] AddedRegisterProcesses = { new CommandProcess(), new ItemProcess(), new MiniGameProcess(), new RoleProcess(), new TeamProcess() }; 

        internal void Init()
        {
            var processors = SynapseController.PluginLoader.GetFieldValueOrPerties<List<IContextProcessor>>("_processors");
            processors.RemoveAll(p => p is Synapse.Api.Plugin.Processors.CommandProcessor);
            foreach (var addProcesses in AddedRegisterProcesses)
                processors.Add(addProcesses);
        }


        /// <summary>
        /// Ignore Only this class for the AutoRegister
        /// </summary>
        public class Ignore : Attribute
        {

        }
    }
}