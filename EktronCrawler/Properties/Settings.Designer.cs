﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EktronCrawler.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://local.ektron/workarea/webservices/Content.asmx")]
        public string EktronCrawler_EktronWeb_ContentApi_Content {
            get {
                return ((string)(this["EktronCrawler_EktronWeb_ContentApi_Content"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://local.ektron/workarea/webservices/folder.asmx")]
        public string EktronCrawler_EktronWeb_FolderApi_Folder {
            get {
                return ((string)(this["EktronCrawler_EktronWeb_FolderApi_Folder"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://local.ektron/workarea/webservices/WebServiceAPI/metadata.asmx")]
        public string EktronCrawler_EktronWeb_MetaDataApi_Metadata {
            get {
                return ((string)(this["EktronCrawler_EktronWeb_MetaDataApi_Metadata"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://local.ektron/workarea/webservices/WebServiceAPI/Taxonomy/taxonomy.asmx")]
        public string EktronCrawler_EktronWeb_TaxonomyApi_Taxonomy {
            get {
                return ((string)(this["EktronCrawler_EktronWeb_TaxonomyApi_Taxonomy"]));
            }
        }
    }
}