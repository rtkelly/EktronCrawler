﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EktronCrawler.AssetTransferServiceReference {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="PingResponse", Namespace="http://schemas.datacontract.org/2004/07/Ektron.Cms.Search.Assets.Server.Data")]
    [System.SerializableAttribute()]
    public partial class PingResponse : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string StatusField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Status {
            get {
                return this.StatusField;
            }
            set {
                if ((object.ReferenceEquals(this.StatusField, value) != true)) {
                    this.StatusField = value;
                    this.RaisePropertyChanged("Status");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="AssetTransferServiceReference.IAssetTransferServer")]
    public interface IAssetTransferServer {
        
        // CODEGEN: Generating message contract since the wrapper name (GetAssetRequest) of message GetAssetRequest does not match the default value (GetAsset)
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IAssetTransferServer/GetAsset", ReplyAction="http://tempuri.org/IAssetTransferServer/GetAssetResponse")]
        EktronCrawler.AssetTransferServiceReference.GetAssetResponse GetAsset(EktronCrawler.AssetTransferServiceReference.GetAssetRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IAssetTransferServer/GetAsset", ReplyAction="http://tempuri.org/IAssetTransferServer/GetAssetResponse")]
        System.Threading.Tasks.Task<EktronCrawler.AssetTransferServiceReference.GetAssetResponse> GetAssetAsync(EktronCrawler.AssetTransferServiceReference.GetAssetRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IAssetTransferServer/Ping", ReplyAction="http://tempuri.org/IAssetTransferServer/PingResponse")]
        EktronCrawler.AssetTransferServiceReference.PingResponse Ping();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IAssetTransferServer/Ping", ReplyAction="http://tempuri.org/IAssetTransferServer/PingResponse")]
        System.Threading.Tasks.Task<EktronCrawler.AssetTransferServiceReference.PingResponse> PingAsync();
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="GetAssetRequest", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class GetAssetRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public System.DateTime LastWriteUtc;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=1)]
        public string Location;
        
        public GetAssetRequest() {
        }
        
        public GetAssetRequest(System.DateTime LastWriteUtc, string Location) {
            this.LastWriteUtc = LastWriteUtc;
            this.Location = Location;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="GetAssetResponse", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class GetAssetResponse {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="http://tempuri.org/")]
        public System.DateTime LastWrite;
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="http://tempuri.org/")]
        public long Size;
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="http://tempuri.org/")]
        public string Status;
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="http://tempuri.org/")]
        public bool Success;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public System.IO.Stream Data;
        
        public GetAssetResponse() {
        }
        
        public GetAssetResponse(System.DateTime LastWrite, long Size, string Status, bool Success, System.IO.Stream Data) {
            this.LastWrite = LastWrite;
            this.Size = Size;
            this.Status = Status;
            this.Success = Success;
            this.Data = Data;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IAssetTransferServerChannel : EktronCrawler.AssetTransferServiceReference.IAssetTransferServer, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class AssetTransferServerClient : System.ServiceModel.ClientBase<EktronCrawler.AssetTransferServiceReference.IAssetTransferServer>, EktronCrawler.AssetTransferServiceReference.IAssetTransferServer {
        
        public AssetTransferServerClient() {
        }
        
        public AssetTransferServerClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public AssetTransferServerClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public AssetTransferServerClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public AssetTransferServerClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        EktronCrawler.AssetTransferServiceReference.GetAssetResponse EktronCrawler.AssetTransferServiceReference.IAssetTransferServer.GetAsset(EktronCrawler.AssetTransferServiceReference.GetAssetRequest request) {
            return base.Channel.GetAsset(request);
        }
        
        public System.DateTime GetAsset(System.DateTime LastWriteUtc, string Location, out long Size, out string Status, out bool Success, out System.IO.Stream Data) {
            EktronCrawler.AssetTransferServiceReference.GetAssetRequest inValue = new EktronCrawler.AssetTransferServiceReference.GetAssetRequest();
            inValue.LastWriteUtc = LastWriteUtc;
            inValue.Location = Location;
            EktronCrawler.AssetTransferServiceReference.GetAssetResponse retVal = ((EktronCrawler.AssetTransferServiceReference.IAssetTransferServer)(this)).GetAsset(inValue);
            Size = retVal.Size;
            Status = retVal.Status;
            Success = retVal.Success;
            Data = retVal.Data;
            return retVal.LastWrite;
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<EktronCrawler.AssetTransferServiceReference.GetAssetResponse> EktronCrawler.AssetTransferServiceReference.IAssetTransferServer.GetAssetAsync(EktronCrawler.AssetTransferServiceReference.GetAssetRequest request) {
            return base.Channel.GetAssetAsync(request);
        }
        
        public System.Threading.Tasks.Task<EktronCrawler.AssetTransferServiceReference.GetAssetResponse> GetAssetAsync(System.DateTime LastWriteUtc, string Location) {
            EktronCrawler.AssetTransferServiceReference.GetAssetRequest inValue = new EktronCrawler.AssetTransferServiceReference.GetAssetRequest();
            inValue.LastWriteUtc = LastWriteUtc;
            inValue.Location = Location;
            return ((EktronCrawler.AssetTransferServiceReference.IAssetTransferServer)(this)).GetAssetAsync(inValue);
        }
        
        public EktronCrawler.AssetTransferServiceReference.PingResponse Ping() {
            return base.Channel.Ping();
        }
        
        public System.Threading.Tasks.Task<EktronCrawler.AssetTransferServiceReference.PingResponse> PingAsync() {
            return base.Channel.PingAsync();
        }
    }
}
