﻿using Synapse.Api.Plugin;
using System;
using VT_Api.Core.Plugin;

/* 
 *      Original Example by GrafDimenzio
 * 
 * https://github.com/SynapseSL/Example-Plugin
 * 
 * 
 * the plugin has been modified to use the VT-API
 */

namespace Exemple_Plugin
{
    [PluginInformation(
            Author = "VT",
            Description = "Example",
            LoadPriority = 0,
            Name = "ExamplePlugin",
            SynapseMajor = 2,
            SynapseMinor = 8,
            SynapsePatch = 2,
            Version = "v1.0.0"
            )]
    public class PluginClass : VtAbstractPlugin<EventHandlers,PluginConfig,PluginTranslation>
    {
        //If it was on true, the CustomRole, CustomTeam
        public override bool AutoRegister => true;

        //It was possible to override the Config to change the section of the config, if you dont override the config takes the name of the plugin.
        [Config(section = "Example VT-Plugin")]
        public override PluginConfig Config { get => base.Config; protected set => base.Config = value; }

        public override void Load()
        {

            Translation.AddTranslation(new PluginTranslation()
            {
                ClassName = "Espion"
            }, "FRENCH");
            Translation.AddTranslation(new PluginTranslation()
            {
                ClassName = "Spion"
            }, "GERMAN");

            // the base load init the API, create the EventHandlers and the instance of the plugin 
            base.Load();
        }

        //This Method can be use for plugins which should close connections or stop a task, it was call when the serveur shut down
        public override void Unload(object sender, EventArgs e)
        {

        }

        //This Method is only needed if you want to reload anything(Translation and Config will be reloaded by Synapse!)
        public override void ReloadConfigs()
        {

        }
    }
}
